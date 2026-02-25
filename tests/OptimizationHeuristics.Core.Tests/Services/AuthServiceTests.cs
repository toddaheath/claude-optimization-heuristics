using FluentAssertions;
using NSubstitute;
using OptimizationHeuristics.Core.Entities;
using OptimizationHeuristics.Core.Services;

namespace OptimizationHeuristics.Core.Tests.Services;

public class AuthServiceTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<RefreshToken> _tokenRepo;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _userRepo = Substitute.For<IRepository<User>>();
        _tokenRepo = Substitute.For<IRepository<RefreshToken>>();
        _tokenService = Substitute.For<ITokenService>();
        _passwordHasher = Substitute.For<IPasswordHasher>();

        _unitOfWork.Repository<User>().Returns(_userRepo);
        _unitOfWork.Repository<RefreshToken>().Returns(_tokenRepo);

        _tokenService.GenerateAccessToken(Arg.Any<User>()).Returns("access-token");
        _tokenService.GenerateRefreshToken().Returns("refresh-token");

        _service = new AuthService(_unitOfWork, _tokenService, _passwordHasher, refreshTokenExpiryDays: 7);
    }

    [Fact]
    public async Task RegisterAsync_NewEmail_ReturnsTokens()
    {
        _userRepo.FindOneAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns((User?)null);
        _passwordHasher.Hash(Arg.Any<string>()).Returns("hashed-pw");

        var result = await _service.RegisterAsync("test@example.com", "Password1", "Test User");

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access-token");
        result.Value.RefreshToken.Should().Be("refresh-token");
        await _unitOfWork.Received().SaveChangesAsync();
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ReturnsFail()
    {
        _userRepo.FindOneAsync(Arg.Any<Expression<Func<User, bool>>>())
            .Returns(new User { Email = "test@example.com" });

        var result = await _service.RegisterAsync("test@example.com", "Password1", "Test");

        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Contain("Email already registered");
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTokens()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", PasswordHash = "hash", IsActive = true };
        _userRepo.FindOneAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(user);
        _passwordHasher.Verify("Password1", "hash").Returns(true);

        var result = await _service.LoginAsync("test@example.com", "Password1");

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access-token");
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsFail()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", PasswordHash = "hash", IsActive = true };
        _userRepo.FindOneAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(user);
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var result = await _service.LoginAsync("test@example.com", "WrongPassword");

        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Contain("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ReturnsFail()
    {
        _userRepo.FindOneAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns((User?)null);

        var result = await _service.LoginAsync("nobody@example.com", "Password1");

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshAsync_ValidToken_ReturnsNewTokens()
    {
        var userId = Guid.NewGuid();
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = "old-refresh",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };
        var user = new User { Id = userId, Email = "test@example.com", IsActive = true };

        _tokenRepo.FindOneAsync(Arg.Any<Expression<Func<RefreshToken, bool>>>()).Returns(token);
        _userRepo.FindOneAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(user);
        _tokenService.GenerateRefreshToken().Returns("new-refresh-token");
        _tokenService.GenerateAccessToken(Arg.Any<User>()).Returns("new-access-token");

        var result = await _service.RefreshAsync("old-refresh");

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("new-access-token");
        result.Value.RefreshToken.Should().Be("new-refresh-token");
        token.IsRevoked.Should().BeTrue();
        token.ReplacedByToken.Should().Be("new-refresh-token");
    }

    [Fact]
    public async Task RefreshAsync_RevokedToken_ReturnsFail()
    {
        var token = new RefreshToken
        {
            Token = "old-refresh",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = true
        };
        _tokenRepo.FindOneAsync(Arg.Any<Expression<Func<RefreshToken, bool>>>()).Returns(token);

        var result = await _service.RefreshAsync("old-refresh");

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshAsync_ReuseAttack_RevokesAllSessionsAndReturnsFail()
    {
        var userId = Guid.NewGuid();
        // Token that was already replaced (reuse)
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = "reused-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            ReplacedByToken = "already-rotated"
        };

        var activeTokens = new List<RefreshToken>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, IsRevoked = false }
        };

        _tokenRepo.FindOneAsync(Arg.Any<Expression<Func<RefreshToken, bool>>>()).Returns(token);
        _tokenRepo.FindAsync(Arg.Any<Expression<Func<RefreshToken, bool>>>()).Returns(activeTokens);

        var result = await _service.RefreshAsync("reused-token");

        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Contain("reuse detected");
        activeTokens[0].IsRevoked.Should().BeTrue();
        await _unitOfWork.Received().SaveChangesAsync();
    }
}
