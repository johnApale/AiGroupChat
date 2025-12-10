using System.Net;
using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Controllers.Auth;

public class ForgotPasswordTests : IntegrationTestBase
{
    public ForgotPasswordTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task ForgotPassword_WithExistingEmail_SendsResetEmail()
    {
        // Arrange
        await Auth.RegisterAndConfirmAsync(email: "user@example.com");
        int emailCountAfterRegister = EmailProvider.SentEmails.Count;

        // Act
        ForgotPasswordRequest request = new()
        {
            Email = "user@example.com"
        };
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/forgot-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(emailCountAfterRegister + 1, EmailProvider.SentEmails.Count);
        Assert.Equal("user@example.com", EmailProvider.LastEmail?.To);
        Assert.Contains("reset", EmailProvider.LastEmail?.Subject, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ForgotPassword_WithNonExistentEmail_Returns200NoEmail()
    {
        // Arrange
        int initialEmailCount = EmailProvider.SentEmails.Count;

        // Act
        ForgotPasswordRequest request = new()
        {
            Email = "nonexistent@example.com"
        };
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/forgot-password", request);

        // Assert - Returns 200 to prevent email enumeration
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(initialEmailCount, EmailProvider.SentEmails.Count);
    }

    [Fact]
    public async Task ForgotPassword_WithUnconfirmedEmail_Returns200AndSendsEmail()
    {
        // Arrange - Register but don't confirm
        await Auth.RegisterAsync(email: "unconfirmed@example.com");
        int emailCountAfterRegister = EmailProvider.SentEmails.Count; // Should be 1 (confirmation email)

        // Act
        ForgotPasswordRequest request = new()
        {
            Email = "unconfirmed@example.com"
        };
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/forgot-password", request);

        // Assert - Returns 200 and sends password reset email
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(emailCountAfterRegister + 1, EmailProvider.SentEmails.Count); // Password reset email sent
    }

    [Fact]
    public async Task ForgotPassword_WithInvalidEmailFormat_Returns400()
    {
        // Act
        ForgotPasswordRequest request = new()
        {
            Email = "not-an-email"
        };
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/forgot-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_WithEmptyEmail_Returns400()
    {
        // Act
        ForgotPasswordRequest request = new()
        {
            Email = ""
        };
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/forgot-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}