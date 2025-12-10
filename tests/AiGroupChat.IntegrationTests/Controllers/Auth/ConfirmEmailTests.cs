using System.Net;
using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Controllers.Auth;

public class ConfirmEmailTests : IntegrationTestBase
{
    public ConfirmEmailTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task ConfirmEmail_WithValidToken_ReturnsTokensAndUser()
    {
        // Arrange
        await Auth.RegisterAsync(
            email: "user@example.com",
            userName: "testuser",
            displayName: "Test User",
            password: "TestPass123!");

        string? token = EmailProvider.ExtractTokenFromLastEmail();
        Assert.NotNull(token);

        // Act
        ConfirmEmailRequest request = new()
        {
            Email = "user@example.com",
            Token = token
        };
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/confirm-email", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        AuthResponse? authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(authResponse);
        Assert.NotEmpty(authResponse.AccessToken);
        Assert.NotEmpty(authResponse.RefreshToken);
        Assert.Equal("user@example.com", authResponse.User.Email);
    }

    [Fact]
    public async Task ConfirmEmail_WithInvalidToken_Returns400()
    {
        // Arrange
        await Auth.RegisterAsync(email: "user@example.com");

        // Act
        ConfirmEmailRequest request = new()
        {
            Email = "user@example.com",
            Token = "invalid-token"
        };
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/confirm-email", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ConfirmEmail_WithNonExistentEmail_Returns400()
    {
        // Act - API returns BadRequest for invalid confirmation requests
        ConfirmEmailRequest request = new()
        {
            Email = "nonexistent@example.com",
            Token = "some-token"
        };
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/confirm-email", request);

        // Assert - Returns 400 BadRequest (generic error to prevent user enumeration)
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ConfirmEmail_WhenAlreadyConfirmed_ReturnsOk()
    {
        // Arrange - Register and confirm email
        EmailProvider.Clear();
        await Auth.RegisterAsync(email: "user@example.com");
        string? token = EmailProvider.ExtractTokenFromLastEmail();
        Assert.NotNull(token);
        
        // First confirmation
        await Auth.ConfirmEmailAsync(email: "user@example.com");
        
        // Act - Second confirmation with same token (idempotent)
        ConfirmEmailRequest request = new()
        {
            Email = "user@example.com",
            Token = token
        };
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/confirm-email", request);

        // Assert - ASP.NET Identity returns success for already confirmed emails
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ConfirmEmail_WithMissingToken_Returns400()
    {
        // Act
        ConfirmEmailRequest request = new()
        {
            Email = "user@example.com",
            Token = ""
        };
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/confirm-email", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}