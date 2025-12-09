using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.Models;
using AiGroupChat.Domain.Entities;
using Moq;

namespace AiGroupChat.UnitTests.Services.AuthService;

public class ResendConfirmationAsyncTests : AuthServiceTestBase
{
    [Fact]
    public async Task WithUnconfirmedUser_SendsEmailAndReturnsMessage()
    {
        // Arrange
        ResendConfirmationRequest request = new ResendConfirmationRequest
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
            .Setup(x => x.IsEmailConfirmedAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        UserRepositoryMock
            .Setup(x => x.GenerateEmailConfirmationTokenAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync("confirmation-token");

        EmailServiceMock
            .Setup(x => x.SendConfirmationEmailAsync(
                request.Email,
                user.DisplayName,
                "confirmation-token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmailResult.Success("email-id"));

        // Act
        MessageResponse result = await AuthService.ResendConfirmationAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("confirmation link has been sent", result.Message);

        EmailServiceMock.Verify(
            x => x.SendConfirmationEmailAsync(
                request.Email,
                user.DisplayName,
                "confirmation-token",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task WithAlreadyConfirmedUser_DoesNotSendEmail()
    {
        // Arrange
        ResendConfirmationRequest request = new ResendConfirmationRequest
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
            .Setup(x => x.IsEmailConfirmedAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        MessageResponse result = await AuthService.ResendConfirmationAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("confirmation link has been sent", result.Message);

        EmailServiceMock.Verify(
            x => x.SendConfirmationEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task WithNonexistentEmail_ReturnsSuccessToPreventEnumeration()
    {
        // Arrange
        ResendConfirmationRequest request = new ResendConfirmationRequest
        {
            Email = "nonexistent@example.com"
        };

        UserRepositoryMock
            .Setup(x => x.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        MessageResponse result = await AuthService.ResendConfirmationAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("confirmation link has been sent", result.Message);

        EmailServiceMock.Verify(
            x => x.SendConfirmationEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}