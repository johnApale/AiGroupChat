using System.Net;
using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Controllers.Auth;

public class RegisterTests : IntegrationTestBase
{
    public RegisterTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Register_WithValidData_Returns201AndSendsConfirmationEmail()
    {
        // Act
        HttpResponseMessage response = await Auth.RegisterAsync(
            email: "newuser@example.com",
            userName: "newuser",
            displayName: "New User",
            password: "SecurePass123!");

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        MessageResponse? content = await response.Content.ReadFromJsonAsync<MessageResponse>();
        Assert.NotNull(content);
        Assert.Contains("check your email", content.Message, StringComparison.OrdinalIgnoreCase);

        // Verify email was sent
        Assert.Single(EmailProvider.SentEmails);
        Assert.Equal("newuser@example.com", EmailProvider.LastEmail?.To);
        Assert.Contains("confirm", EmailProvider.LastEmail?.Subject, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns400()
    {
        // Arrange
        await Auth.RegisterAsync(
            email: "duplicate@example.com",
            userName: "user1",
            displayName: "User One");

        // Act
        HttpResponseMessage response = await Auth.RegisterAsync(
            email: "duplicate@example.com",
            userName: "user2",
            displayName: "User Two");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateUserName_Returns400()
    {
        // Arrange
        await Auth.RegisterAsync(
            email: "user1@example.com",
            userName: "sameusername",
            displayName: "User One");

        // Act
        HttpResponseMessage response = await Auth.RegisterAsync(
            email: "user2@example.com",
            userName: "sameusername",
            displayName: "User Two");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_Returns400()
    {
        // Act
        HttpResponseMessage response = await Auth.RegisterAsync(
            email: "not-an-email",
            userName: "testuser",
            displayName: "Test User");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithShortUserName_Returns400()
    {
        // Act
        HttpResponseMessage response = await Auth.RegisterAsync(
            email: "test@example.com",
            userName: "ab",  // Less than 3 characters
            displayName: "Test User");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithWeakPassword_Returns400()
    {
        // Act
        HttpResponseMessage response = await Auth.RegisterAsync(
            email: "test@example.com",
            userName: "testuser",
            displayName: "Test User",
            password: "weak");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}