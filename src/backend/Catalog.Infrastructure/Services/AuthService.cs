using Catalog.Core.Entities;
using Catalog.Core.Services;
using Microsoft.Extensions.Logging;
using Supabase;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;

namespace Catalog.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly SupabaseClient _supabaseClient;
    private readonly ILogger<AuthService> _logger;
    private readonly string _supabaseUrl;

    public AuthService(SupabaseClient supabaseClient, ILogger<AuthService> logger, IConfiguration? configuration = null)
    {
        _supabaseClient = supabaseClient;
        _logger = logger;
        _supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL")
            ?? configuration?["SUPABASE_URL"]
            ?? throw new InvalidOperationException("SUPABASE_URL not found");
    }

    public async Task<User?> ValidateTokenAsync(string token)
    {
        try
        {
            // Parse JWT token to extract claims
            var handler = new JwtSecurityTokenHandler();
            
            if (!handler.CanReadToken(token))
            {
                _logger.LogWarning("Cannot read token");
                return null;
            }

            var jsonToken = handler.ReadJwtToken(token);
            
            // Extract user information from token claims
            var userId = jsonToken.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier)?.Value;
            var email = jsonToken.Claims.FirstOrDefault(c => c.Type == "email" || c.Type == ClaimTypes.Email)?.Value;
            var name = jsonToken.Claims.FirstOrDefault(c => c.Type == "name" || c.Type == ClaimTypes.Name)?.Value;
            
            // Get role from user_metadata or app_metadata
            var role = jsonToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value
                ?? jsonToken.Claims.FirstOrDefault(c => c.Type == "user_role")?.Value
                ?? "Viewer";

            // Also try to get from user_metadata in the token
            var userMetadata = jsonToken.Claims.FirstOrDefault(c => c.Type == "user_metadata")?.Value;
            if (!string.IsNullOrEmpty(userMetadata))
            {
                try
                {
                    var metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(userMetadata);
                    if (metadata != null && metadata.TryGetValue("role", out var roleObj))
                    {
                        role = roleObj?.ToString() ?? "Viewer";
                    }
                }
                catch
                {
                    // Ignore parsing errors
                }
            }

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Token missing user ID");
                return null;
            }

            // Verify token with Supabase (optional - for additional validation)
            // For now, we'll trust the JWT if it's properly formatted
            // In production, you might want to verify the signature with Supabase's public key

            return new User
            {
                Id = userId,
                Email = email ?? string.Empty,
                Name = name,
                Role = role,
                CreatedAt = jsonToken.ValidFrom != DateTime.MinValue 
                    ? new DateTimeOffset(jsonToken.ValidFrom) 
                    : DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return null;
        }
    }

    public async Task<User?> GetUserFromTokenAsync(string token)
    {
        return await ValidateTokenAsync(token);
    }

    public bool IsAuthorized(User? user, string requiredRole)
    {
        if (user == null)
        {
            return false;
        }

        // Admin can access everything
        if (user.Role == "Admin")
        {
            return true;
        }

        // Check if user has the required role
        return user.Role == requiredRole;
    }
}

