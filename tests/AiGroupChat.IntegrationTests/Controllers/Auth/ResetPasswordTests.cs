using System.Net;
using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Controllers.Auth;

public class ResetPasswordTests : IntegrationTestBase
{
    public ResetPasswordTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task ResetPassword_WithValidToken_ResetsPasswordSuccessfully()
    {
        // Arrange
        await Auth.RegisterAndConfirmAsync(
            email: "user@example.com",
            password: "OldPassword123!");

        // Request password reset
        ForgotPasswordRequest forgotRequest = new()
        {
            Email = "user@example.com"
        };
        await Client.PostAsJsonAsync("/api/auth/forgot-password", forgotRequest);

        string? resetToken = EmailProvider.ExtractTokenFromLastEmail();
        Assert.NotNull(resetToken);

        // Act
        ResetPasswordRequest resetRequest = new()
        {
            Email = "user@example.com",
            Token = resetToken,
            NewPassword = "NewPassword123!"
        };
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/reset-password", resetRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify can login with new password
        AuthResponse loginResponse = await Auth.LoginAsync("user@example.com", "NewPassword123!");
        Assert.NotEmpty(loginResponse.AccessToken);
    }

    [Fact]
    public async Task ResetPassword_WithValidToken_OldPasswordNoLongerWorks()
    {
        // Arrange
        await Auth.RegisterAndConfirmAsync(
            email: "user@example.com",
            password: "OldPassword123!");

        // Request password reset
        ForgotPasswordRequest forgotRequest = new()
        {
            Email = "user@example.com"
        };
        await Client.PostAsJsonAsync("/api/auth/forgot-password", forgotRequest);

        string? resetToken = EmailProvider.ExtractTokenFromLastEmail();
        Assert.NotNull(resetToken);

        // Reset password
        ResetPasswordRequest resetRequest = new()
        {
            Email = "user@example.com",
            Token = resetToken,
            NewPassword = "NewPassword123!"
        };
        await Client.PostAsJsonAsync("/api/auth/reset-password", resetRequest);

        // Act - Try to login with old password
        HttpResponseMessage response = await Auth.LoginRawAsync("user@example.com", "OldPassword123!");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithInvalidToken_Returns400()
    {
        // Arrange
        await Auth.RegisterAndConfirmAsync(email: "user@example.com");

        // Act
        ResetPasswordRequest request = new()
        {
            Email = "user@example.com",
            Token = "invalid-token",
            NewPassword = "NewPassword123!"
        };
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/reset-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithNonExistentEmail_Returns400()
    {
        // Act - API returns BadRequest for invalid reset requests (prevents user enumeration)
        ResetPasswordRequest request = new()
        {
            Email = "nonexistent@example.com",
            Token = "some-token",
            NewPassword = "NewPass123!"
        };
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/reset-password", request);

        // Assert - Returns 400 BadRequest to prevent revealing if email exists
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithWeakPassword_Returns400()
    {
        // Arrange
        await Auth.RegisterAndConfirmAsync(email: "user@example.com");

        ForgotPasswordRequest forgotRequest = new()
        {
            Email = "user@example.com"
        };
        await Client.PostAsJsonAsync("/api/auth/forgot-password", forgotRequest);

        string? resetToken = EmailProvider.ExtractTokenFromLastEmail();
        Assert.NotNull(resetToken);

        // Act
        ResetPasswordRequest resetRequest = new()
        {
            Email = "user@example.com",
            Token = resetToken,
            NewPassword = "weak"
        };
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/reset-password", resetRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_TokenCanOnlyBeUsedOnce()
    {
        // Arrange
        await Auth.RegisterAndConfirmAsync(email: "user@example.com");

        ForgotPasswordRequest forgotRequest = new()
        {
            Email = "user@example.com"
        };
        await Client.PostAsJsonAsync("/api/auth/forgot-password", forgotRequest);

        string? resetToken = EmailProvider.ExtractTokenFromLastEmail();
        Assert.NotNull(resetToken);

        // Use token first time
        ResetPasswordRequest resetRequest = new()
        {
            Email = "user@example.com",
            Token = resetToken,
            NewPassword = "NewPassword123!"
        };
        await Client.PostAsJsonAsync("/api/auth/reset-password", resetRequest);

        // Act - Try to use same token again
        resetRequest.NewPassword = "AnotherPassword123!";
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/reset-password", resetRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}