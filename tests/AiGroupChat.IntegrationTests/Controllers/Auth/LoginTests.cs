using System.Net;
using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Controllers.Auth;

public class LoginTests : IntegrationTestBase
{
    public LoginTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokensAndUser()
    {
        // Arrange
        await Auth.RegisterAndConfirmAsync(
            email: "user@example.com",
            userName: "testuser",
            displayName: "Test User",
            password: "TestPass123!");

        // Act
        AuthResponse response = await Auth.LoginAsync("user@example.com", "TestPass123!");

        // Assert
        Assert.NotEmpty(response.AccessToken);
        Assert.NotEmpty(response.RefreshToken);
        Assert.True(response.ExpiresAt > DateTime.UtcNow);
        Assert.Equal("user@example.com", response.User.Email);
        Assert.Equal("testuser", response.User.UserName);
        Assert.Equal("Test User", response.User.DisplayName);
    }

    [Fact]
    public async Task Login_WithUnconfirmedEmail_Returns401()
    {
        // Arrange - Register but don't confirm
        await Auth.RegisterAsync(
            email: "unconfirmed@example.com",
            userName: "unconfirmed",
            password: "TestPass123!");

        // Act
        HttpResponseMessage response = await Auth.LoginRawAsync("unconfirmed@example.com", "TestPass123!");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        // Arrange
        await Auth.RegisterAndConfirmAsync(
            email: "user@example.com",
            password: "TestPass123!");

        // Act
        HttpResponseMessage response = await Auth.LoginRawAsync("user@example.com", "WrongPassword123!");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_Returns401()
    {
        // Act
        HttpResponseMessage response = await Auth.LoginRawAsync("nonexistent@example.com", "TestPass123!");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidEmailFormat_Returns400()
    {
        // Act
        HttpResponseMessage response = await Auth.LoginRawAsync("not-an-email", "TestPass123!");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithEmptyPassword_Returns400()
    {
        // Act
        HttpResponseMessage response = await Auth.LoginRawAsync("user@example.com", "");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}