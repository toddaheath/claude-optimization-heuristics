using OptimizationHeuristics.Core.Entities;

namespace OptimizationHeuristics.Core.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
