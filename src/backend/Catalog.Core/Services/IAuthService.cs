namespace Catalog.Core.Services;

public interface IAuthService
{
    Task<User?> ValidateTokenAsync(string token);
    Task<User?> GetUserFromTokenAsync(string token);
    bool IsAuthorized(User? user, string requiredRole);
}

