using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptimizationHeuristics.Api.DTOs;
using OptimizationHeuristics.Api.Extensions;
using OptimizationHeuristics.Core.Services;

namespace OptimizationHeuristics.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request.Email, request.Password, request.DisplayName);
        return result.Map(tokens => new AuthResponse(tokens.AccessToken, tokens.RefreshToken, tokens.RefreshTokenExpiry))
            .ToActionResult();
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password);
        return result.Map(tokens => new AuthResponse(tokens.AccessToken, tokens.RefreshToken, tokens.RefreshTokenExpiry))
            .ToActionResult();
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var result = await _authService.RefreshAsync(request.RefreshToken);
        return result.Map(tokens => new AuthResponse(tokens.AccessToken, tokens.RefreshToken, tokens.RefreshTokenExpiry))
            .ToActionResult();
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<ActionResult> Revoke([FromBody] RevokeRequest request)
    {
        var result = await _authService.RevokeAsync(request.RefreshToken);
        return result.ToActionResult();
    }
}
