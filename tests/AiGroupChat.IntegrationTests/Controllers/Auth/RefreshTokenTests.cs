using System.Net;
using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Controllers.Auth;

public class RefreshTokenTests : IntegrationTestBase
{
    public RefreshTokenTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsNewTokens()
    {
        // Arrange
        AuthResponse authResponse = await Auth.RegisterAndConfirmAsync(email: "user@example.com");
        string originalRefreshToken = authResponse.RefreshToken;

        // Act
        RefreshTokenRequest request = new()
        {
            RefreshToken = originalRefreshToken
        };
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/refresh", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        AuthResponse? newAuthResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(newAuthResponse);
        Assert.NotEmpty(newAuthResponse.AccessToken);
        Assert.NotEmpty(newAuthResponse.RefreshToken);
        Assert.NotEqual(originalRefreshToken, newAuthResponse.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_Returns401()
    {
        // Act
        RefreshTokenRequest request = new()
        {
            RefreshToken = "invalid-refresh-token"
        };
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/refresh", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_WithEmptyToken_Returns400()
    {
        // Act
        RefreshTokenRequest request = new()
        {
            RefreshToken = ""
        };
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/refresh", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_OldTokenInvalidatedAfterRefresh()
    {
        // Arrange
        AuthResponse authResponse = await Auth.RegisterAndConfirmAsync(email: "user@example.com");
        string originalRefreshToken = authResponse.RefreshToken;

        // Use the refresh token
        RefreshTokenRequest request = new()
        {
            RefreshToken = originalRefreshToken
        };
        await Client.PostAsJsonAsync("/api/auth/refresh", request);

        // Act - Try to use the old token again
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/refresh", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_NewTokenCanBeUsed()
    {
        // Arrange
        AuthResponse authResponse = await Auth.RegisterAndConfirmAsync(email: "user@example.com");

        // Get new tokens
        RefreshTokenRequest request = new()
        {
            RefreshToken = authResponse.RefreshToken
        };
        HttpResponseMessage firstRefreshResponse = await Client.PostAsJsonAsync("/api/auth/refresh", request);
        AuthResponse? newAuthResponse = await firstRefreshResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(newAuthResponse);

        // Act - Use the new refresh token
        request.RefreshToken = newAuthResponse.RefreshToken;
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/refresh", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_NewAccessTokenWorksForAuthenticatedEndpoints()
    {
        // Arrange
        AuthResponse authResponse = await Auth.RegisterAndConfirmAsync(email: "user@example.com");

        // Get new tokens
        RefreshTokenRequest request = new()
        {
            RefreshToken = authResponse.RefreshToken
        };
        HttpResponseMessage refreshResponse = await Client.PostAsJsonAsync("/api/auth/refresh", request);
        AuthResponse? newAuthResponse = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(newAuthResponse);

        // Act - Use new access token for authenticated request
        Auth.SetAuthToken(newAuthResponse.AccessToken);
        HttpResponseMessage response = await Client.GetAsync("/api/users/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}