using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.Models;
using AiGroupChat.Domain.Entities;
using Moq;

namespace AiGroupChat.UnitTests.Services.AuthService;

public class ForgotPasswordAsyncTests : AuthServiceTestBase
{
    [Fact]
    public async Task WithExistingUser_SendsResetEmailAndReturnsMessage()
    {
        // Arrange
        ForgotPasswordRequest request = new ForgotPasswordRequest
        {
            Email = "test@example.com"
        };

        User user = new User
        {
            Id = "user-id-123",
            Email = request.Email,
            DisplayName = "Test User"
        };

        UserRepositoryMock
            .Setup(x => x.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        UserRepositoryMock
            .Setup(x => x.GeneratePasswordResetTokenAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync("reset-token");

        EmailServiceMock
            .Setup(x => x.SendPasswordResetEmailAsync(
                request.Email,
                user.DisplayName,
                "reset-token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmailResult.Success("email-id"));

        // Act
        MessageResponse result = await AuthService.ForgotPasswordAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("password reset link has been sent", result.Message);

        EmailServiceMock.Verify(
            x => x.SendPasswordResetEmailAsync(
                request.Email,
                user.DisplayName,
                "reset-token",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task WithNonexistentEmail_ReturnsSuccessToPreventEnumeration()
    {
        // Arrange
        ForgotPasswordRequest request = new ForgotPasswordRequest
        {
            Email = "nonexistent@example.com"
        };

        UserRepositoryMock
            .Setup(x => x.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        MessageResponse result = await AuthService.ForgotPasswordAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("password reset link has been sent", result.Message);

        EmailServiceMock.Verify(
            x => x.SendPasswordResetEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}