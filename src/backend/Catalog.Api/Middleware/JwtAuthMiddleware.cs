using System.Net;
using System.Security.Claims;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Catalog.Core.Services;

namespace Catalog.Api.Middleware;

public class JwtAuthMiddleware : IFunctionsWorkerMiddleware
{
    private readonly IAuthService _authService;
    private readonly ILogger<JwtAuthMiddleware> _logger;

    public JwtAuthMiddleware(IAuthService authService, ILogger<JwtAuthMiddleware> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var httpContext = context.GetHttpContext();
        
        if (httpContext == null)
        {
            await next(context);
            return;
        }

        var request = httpContext.Request;
        var response = httpContext.Response;

        // Skip authentication for health check and swagger endpoints
        if (request.Path.StartsWithSegments("/api/health") || 
            request.Path.StartsWithSegments("/swagger") ||
            request.Path == "/")
        {
            await next(context);
            return;
        }

        // Check if the function requires authentication
        var functionMetadata = context.FunctionDefinition;
        var requiresAuth = functionMetadata.InputBindings.Values
            .Any(b => b.Type == "httpTrigger" && 
                     b.Properties.TryGetValue("authLevel", out var authLevel) && 
                     authLevel?.ToString() != "Anonymous");

        // For now, we'll check all endpoints except health/swagger
        // In production, you'd check the function's authorization level attribute
        if (!request.Path.StartsWithSegments("/api/health") && 
            !request.Path.StartsWithSegments("/swagger"))
        {
            // Extract token from Authorization header
            if (!request.Headers.TryGetValue("Authorization", out var authHeader) ||
                string.IsNullOrEmpty(authHeader))
            {
                _logger.LogWarning("Missing Authorization header for {Path}", request.Path);
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.ContentType = "application/json";
                await response.WriteAsJsonAsync(new { error = "Unauthorized", message = "Missing or invalid authorization token" });
                return;
            }

            // Extract token from "Bearer <token>"
            var token = authHeader.ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase).Trim();

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Empty token for {Path}", request.Path);
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.ContentType = "application/json";
                await response.WriteAsJsonAsync(new { error = "Unauthorized", message = "Invalid token format" });
                return;
            }

            // Validate token
            var user = await _authService.ValidateTokenAsync(token);

            if (user == null)
            {
                _logger.LogWarning("Invalid token for {Path}", request.Path);
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.ContentType = "application/json";
                await response.WriteAsJsonAsync(new { error = "Unauthorized", message = "Invalid or expired token" });
                return;
            }

            // Add user to context for use in functions
            context.Items["User"] = user;
            context.Items["UserId"] = user.Id;
            context.Items["UserRole"] = user.Role;
        }

        await next(context);
    }
}

