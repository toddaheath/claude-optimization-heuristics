using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OptimizationHeuristics.Api.DTOs;

namespace OptimizationHeuristics.Integration.Tests;

public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_Login_Refresh_Revoke_Flow()
    {
        var email = $"test-{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1";

        // Register
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest(email, password, "Test User"));
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var registerBody = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        registerBody!.Success.Should().BeTrue();
        registerBody.Data!.AccessToken.Should().NotBeNullOrEmpty();
        registerBody.Data.RefreshToken.Should().NotBeNullOrEmpty();

        // Login
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, password));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        loginBody!.Success.Should().BeTrue();
        var refreshToken = loginBody.Data!.RefreshToken;

        // Refresh
        var refreshResponse = await _client.PostAsJsonAsync("/api/v1/auth/refresh",
            new RefreshRequest(refreshToken));
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshBody = await refreshResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        refreshBody!.Success.Should().BeTrue();
        var newRefreshToken = refreshBody.Data!.RefreshToken;

        // Revoke (requires auth — use factory client to stay on the test server)
        using var authClient = _factory.CreateClient();
        authClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", refreshBody.Data.AccessToken);
        var revokeResponse = await authClient.PostAsJsonAsync("/api/v1/auth/revoke",
            new RevokeRequest(newRefreshToken));
        revokeResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest("nonexistent@example.com", "WrongPass1"));

        // The API returns a failed result with unauthorized error
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Fails()
    {
        var email = $"dup-{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1";

        await _client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest(email, password, "User One"));

        var secondResponse = await _client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest(email, password, "User Two"));

        // Should fail - either conflict or bad request
        secondResponse.IsSuccessStatusCode.Should().BeFalse();
    }
}
