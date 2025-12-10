using System.Net;
using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.DTOs.Users;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Controllers.Users;

public class UsersControllerTests : IntegrationTestBase
{
    public UsersControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetCurrentUser_WithValidToken_ReturnsUserInfo()
    {
        // Arrange
        AuthResponse authResponse = await Auth.CreateAuthenticatedUserAsync(
            email: "user@example.com",
            userName: "testuser",
            displayName: "Test User");

        // Act
        HttpResponseMessage response = await Client.GetAsync("/api/users/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        UserResponse? user = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(user);
        Assert.Equal(authResponse.User.Id, user.Id);
        Assert.Equal("user@example.com", user.Email);
        Assert.Equal("testuser", user.UserName);
        Assert.Equal("Test User", user.DisplayName);
        Assert.True(user.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task GetCurrentUser_WithoutToken_Returns401()
    {
        // Arrange - ensure no auth token is set
        Auth.ClearAuthToken();

        // Act
        HttpResponseMessage response = await Client.GetAsync("/api/users/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUser_WithInvalidToken_Returns401()
    {
        // Arrange - set an invalid/malformed token
        Auth.SetAuthToken("invalid-token-that-is-not-a-jwt");

        // Act
        HttpResponseMessage response = await Client.GetAsync("/api/users/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsUserInfo()
    {
        // Arrange - create and authenticate a user
        AuthResponse authResponse = await Auth.CreateAuthenticatedUserAsync(
            email: "user@example.com",
            userName: "testuser",
            displayName: "Test User");

        string userId = authResponse.User.Id;

        // Act
        HttpResponseMessage response = await Client.GetAsync($"/api/users/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        UserResponse? user = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(user);
        Assert.Equal(userId, user.Id);
        Assert.Equal("user@example.com", user.Email);
        Assert.Equal("testuser", user.UserName);
        Assert.Equal("Test User", user.DisplayName);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_Returns404()
    {
        // Arrange - create an authenticated user first (endpoint requires auth)
        await Auth.CreateAuthenticatedUserAsync();

        string nonExistentId = Guid.NewGuid().ToString();

        // Act
        HttpResponseMessage response = await Client.GetAsync($"/api/users/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithoutToken_Returns401()
    {
        // Arrange - ensure no auth token is set
        Auth.ClearAuthToken();

        string someUserId = Guid.NewGuid().ToString();

        // Act
        HttpResponseMessage response = await Client.GetAsync($"/api/users/{someUserId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}