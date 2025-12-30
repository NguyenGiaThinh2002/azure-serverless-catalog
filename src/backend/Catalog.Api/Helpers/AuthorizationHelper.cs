using System.Net;
using Catalog.Core.Entities;
using Catalog.Core.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Catalog.Api.Helpers;

public static class AuthorizationHelper
{
    public static User? GetUserFromContext(FunctionContext context)
    {
        if (context.Items.TryGetValue("User", out var userObj) && userObj is User user)
        {
            return user;
        }
        return null;
    }

    public static bool IsAuthorized(FunctionContext context, string requiredRole, IAuthService authService)
    {
        var user = GetUserFromContext(context);
        return authService.IsAuthorized(user, requiredRole);
    }

    public static async Task<HttpResponseData> CreateUnauthorizedResponseAsync(HttpRequestData req, string message = "Unauthorized")
    {
        var response = req.CreateResponse(HttpStatusCode.Unauthorized);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteAsJsonAsync(new { error = "Unauthorized", message });
        return response;
    }

    public static async Task<HttpResponseData> CreateForbiddenResponseAsync(HttpRequestData req, string message = "Forbidden")
    {
        var response = req.CreateResponse(HttpStatusCode.Forbidden);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteAsJsonAsync(new { error = "Forbidden", message });
        return response;
    }
}

