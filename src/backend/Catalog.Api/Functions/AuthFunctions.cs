using System.Net;
using System.Text.Json;
using Catalog.Core.Entities;
using Catalog.Core.Services;
using Catalog.Api.Helpers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Catalog.Api.Functions;

public class AuthFunctions
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthFunctions> _logger;

    public AuthFunctions(IAuthService authService, ILogger<AuthFunctions> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [Function("GetUserInfo")]
    public async Task<HttpResponseData> GetUserInfo(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "auth/me")] HttpRequestData req,
        FunctionContext context)
    {
        var user = AuthorizationHelper.GetUserFromContext(context);

        if (user == null)
        {
            return await AuthorizationHelper.CreateUnauthorizedResponseAsync(req, "User not authenticated");
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteAsJsonAsync(new
        {
            id = user.Id,
            email = user.Email,
            name = user.Name,
            role = user.Role
        });

        return response;
    }

    [Function("ValidateToken")]
    public async Task<HttpResponseData> ValidateToken(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/validate")] HttpRequestData req,
        FunctionContext context)
    {
        try
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonSerializer.Deserialize<ValidateTokenRequest>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (string.IsNullOrEmpty(request?.Token))
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                errorResponse.Headers.Add("Content-Type", "application/json");
                await errorResponse.WriteAsJsonAsync(new { error = "Token is required" });
                return errorResponse;
            }

            var user = await _authService.ValidateTokenAsync(request.Token);

            if (user == null)
            {
                var invalidResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                invalidResponse.Headers.Add("Content-Type", "application/json");
                await invalidResponse.WriteAsJsonAsync(new { error = "Invalid or expired token" });
                return invalidResponse;
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteAsJsonAsync(new
            {
                valid = true,
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    name = user.Name,
                    role = user.Role
                }
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            errorResponse.Headers.Add("Content-Type", "application/json");
            await errorResponse.WriteAsJsonAsync(new { error = "Internal server error" });
            return errorResponse;
        }
    }

    private class ValidateTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}

