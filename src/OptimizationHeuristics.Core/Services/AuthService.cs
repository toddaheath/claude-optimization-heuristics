using FluentResults;
using OptimizationHeuristics.Core.Entities;

namespace OptimizationHeuristics.Core.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly int _refreshTokenExpiryDays;

    public AuthService(
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        int refreshTokenExpiryDays)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _refreshTokenExpiryDays = refreshTokenExpiryDays;
    }

    public async Task<Result<AuthTokens>> RegisterAsync(string email, string password, string displayName)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var existing = await _unitOfWork.Repository<User>().FindOneAsync(x => x.Email == normalizedEmail);
        if (existing is not null)
            return Result.Fail<AuthTokens>("Email already registered");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.Hash(password),
            DisplayName = displayName,
            IsActive = true
        };

        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return await IssueTokensAsync(user);
    }

    public async Task<Result<AuthTokens>> LoginAsync(string email, string password)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var user = await _unitOfWork.Repository<User>().FindOneAsync(x => x.Email == normalizedEmail && x.IsActive);
        if (user is null || !_passwordHasher.Verify(password, user.PasswordHash))
            return Result.Fail<AuthTokens>("Invalid email or password");

        return await IssueTokensAsync(user);
    }

    public async Task<Result<AuthTokens>> RefreshAsync(string refreshToken)
    {
        var token = await _unitOfWork.Repository<RefreshToken>().FindOneAsync(x => x.Token == refreshToken);
        if (token is null || token.IsRevoked || token.ExpiresAt < DateTime.UtcNow)
            return Result.Fail<AuthTokens>("Token is invalid or expired");

        if (token.ReplacedByToken is not null)
        {
            // Reuse attack detected — revoke all active tokens for this user
            var allTokens = await _unitOfWork.Repository<RefreshToken>()
                .FindAsync(x => x.UserId == token.UserId && !x.IsRevoked);
            foreach (var t in allTokens)
            {
                t.IsRevoked = true;
                t.RevokedReason = "reuse detected";
                _unitOfWork.Repository<RefreshToken>().Update(t);
            }
            await _unitOfWork.SaveChangesAsync();
            return Result.Fail<AuthTokens>("Token reuse detected; all sessions revoked");
        }

        var user = await _unitOfWork.Repository<User>().FindOneAsync(x => x.Id == token.UserId && x.IsActive);
        if (user is null)
            return Result.Fail<AuthTokens>("Token is invalid or expired");

        var newRefreshTokenValue = _tokenService.GenerateRefreshToken();
        token.IsRevoked = true;
        token.ReplacedByToken = newRefreshTokenValue;
        _unitOfWork.Repository<RefreshToken>().Update(token);

        var expiry = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);
        var newToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = newRefreshTokenValue,
            ExpiresAt = expiry
        };
        await _unitOfWork.Repository<RefreshToken>().AddAsync(newToken);
        await _unitOfWork.SaveChangesAsync();

        var accessToken = _tokenService.GenerateAccessToken(user);
        return Result.Ok(new AuthTokens(accessToken, newRefreshTokenValue, expiry));
    }

    public async Task<Result> RevokeAsync(string refreshToken, string reason = "logout")
    {
        var token = await _unitOfWork.Repository<RefreshToken>().FindOneAsync(x => x.Token == refreshToken);
        if (token is null)
            return Result.Ok(); // Idempotent

        if (!token.IsRevoked)
        {
            token.IsRevoked = true;
            token.RevokedReason = reason;
            _unitOfWork.Repository<RefreshToken>().Update(token);
            await _unitOfWork.SaveChangesAsync();
        }

        return Result.Ok();
    }

    private async Task<Result<AuthTokens>> IssueTokensAsync(User user)
    {
        var refreshTokenValue = _tokenService.GenerateRefreshToken();
        var expiry = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = expiry
        };

        await _unitOfWork.Repository<RefreshToken>().AddAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        var accessToken = _tokenService.GenerateAccessToken(user);
        return Result.Ok(new AuthTokens(accessToken, refreshTokenValue, expiry));
    }
}
