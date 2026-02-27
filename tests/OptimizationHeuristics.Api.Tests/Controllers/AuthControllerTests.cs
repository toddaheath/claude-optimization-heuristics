using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using OptimizationHeuristics.Api.Controllers;
using OptimizationHeuristics.Api.DTOs;
using OptimizationHeuristics.Core.Errors;
using OptimizationHeuristics.Core.Services;

namespace OptimizationHeuristics.Api.Tests.Controllers;

public class AuthControllerTests
{
    private readonly IAuthService _authService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authService = Substitute.For<IAuthService>();
        _controller = new AuthController(_authService);
    }

    [Fact]
    public async Task Register_Success_ReturnsOk()
    {
        var expiry = DateTime.UtcNow.AddDays(7);
        _authService.RegisterAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Result.Ok(new AuthTokens("access", "refresh", expiry)));

        var result = await _controller.Register(new RegisterRequest("test@example.com", "Password1", "Test"));

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        _authService.RegisterAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Result.Fail<AuthTokens>("Email already registered"));

        var result = await _controller.Register(new RegisterRequest("dup@example.com", "Password1", "Test"));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        var expiry = DateTime.UtcNow.AddDays(7);
        _authService.LoginAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(Result.Ok(new AuthTokens("access", "refresh", expiry)));

        var result = await _controller.Login(new LoginRequest("test@example.com", "Password1"));

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        _authService.LoginAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(Result.Fail<AuthTokens>(new UnauthorizedError("Invalid email or password")));

        var result = await _controller.Login(new LoginRequest("test@example.com", "wrong"));

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Refresh_ValidToken_ReturnsOk()
    {
        var expiry = DateTime.UtcNow.AddDays(7);
        _authService.RefreshAsync(Arg.Any<string>())
            .Returns(Result.Ok(new AuthTokens("new-access", "new-refresh", expiry)));

        var result = await _controller.Refresh(new RefreshRequest("old-refresh"));

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Refresh_InvalidToken_ReturnsUnauthorized()
    {
        _authService.RefreshAsync(Arg.Any<string>())
            .Returns(Result.Fail<AuthTokens>(new UnauthorizedError("Token is invalid or expired")));

        var result = await _controller.Refresh(new RefreshRequest("bad-token"));

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Revoke_ValidRequest_ReturnsNoContent()
    {
        _authService.RevokeAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(Result.Ok());

        var result = await _controller.Revoke(new RevokeRequest("some-token"));

        result.Should().BeOfType<NoContentResult>();
    }
}
