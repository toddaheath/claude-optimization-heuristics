using FluentResults;

namespace OptimizationHeuristics.Core.Services;

public record AuthTokens(string AccessToken, string RefreshToken, DateTime RefreshTokenExpiry);

public interface IAuthService
{
    Task<Result<AuthTokens>> RegisterAsync(string email, string password, string displayName);
    Task<Result<AuthTokens>> LoginAsync(string email, string password);
    Task<Result<AuthTokens>> RefreshAsync(string refreshToken);
    Task<Result> RevokeAsync(string refreshToken, string reason = "logout");
}
