using System.Net;
using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Controllers.Auth;

public class ResendConfirmationTests : IntegrationTestBase
{
    public ResendConfirmationTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task ResendConfirmation_WithUnconfirmedEmail_SendsNewEmail()
    {
        // Arrange
        await Auth.RegisterAsync(email: "user@example.com");
        int initialEmailCount = EmailProvider.SentEmails.Count;

        // Act
        ResendConfirmationRequest request = new()
        {
            Email = "user@example.com"
        };
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/resend-confirmation", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(initialEmailCount + 1, EmailProvider.SentEmails.Count);
        Assert.Equal("user@example.com", EmailProvider.LastEmail?.To);
    }

    [Fact]
    public async Task ResendConfirmation_WithAlreadyConfirmedEmail_Returns200ButNoEmail()
    {
        // Arrange - Register and confirm
        await Auth.RegisterAndConfirmAsync(email: "confirmed@example.com");
        int emailCountAfterConfirm = EmailProvider.SentEmails.Count;

        // Act
        ResendConfirmationRequest request = new()
        {
            Email = "confirmed@example.com"
        };
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/resend-confirmation", request);

        // Assert - Returns 200 to prevent email enumeration, but no new email sent
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(emailCountAfterConfirm, EmailProvider.SentEmails.Count);
    }

    [Fact]
    public async Task ResendConfirmation_WithNonExistentEmail_Returns200()
    {
        // Arrange
        int initialEmailCount = EmailProvider.SentEmails.Count;

        // Act
        ResendConfirmationRequest request = new()
        {
            Email = "nonexistent@example.com"
        };
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/resend-confirmation", request);

        // Assert - Returns 200 to prevent email enumeration
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(initialEmailCount, EmailProvider.SentEmails.Count);
    }

    [Fact]
    public async Task ResendConfirmation_WithInvalidEmailFormat_Returns400()
    {
        // Act
        ResendConfirmationRequest request = new()
        {
            Email = "not-an-email"
        };
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/resend-confirmation", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ResendConfirmation_NewTokenWorksForConfirmation()
    {
        // Arrange
        await Auth.RegisterAsync(email: "user@example.com");
        
        // Request new confirmation email
        ResendConfirmationRequest resendRequest = new()
        {
            Email = "user@example.com"
        };
        await Client.PostAsJsonAsync("/api/auth/resend-confirmation", resendRequest);

        // Get the new token
        string? newToken = EmailProvider.ExtractTokenFromLastEmail();
        Assert.NotNull(newToken);

        // Act - Confirm with new token
        ConfirmEmailRequest confirmRequest = new()
        {
            Email = "user@example.com",
            Token = newToken
        };
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/confirm-email", confirmRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}