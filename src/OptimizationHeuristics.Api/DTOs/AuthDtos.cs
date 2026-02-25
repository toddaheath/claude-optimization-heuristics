namespace OptimizationHeuristics.Api.DTOs;

public record RegisterRequest(string Email, string Password, string DisplayName);
public record LoginRequest(string Email, string Password);
public record RefreshRequest(string RefreshToken);
public record RevokeRequest(string RefreshToken);
public record AuthResponse(string AccessToken, string RefreshToken, DateTime RefreshTokenExpiry);
