using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.Exceptions;
using AiGroupChat.Domain.Entities;
using Moq;

namespace AiGroupChat.UnitTests.Services.AuthService;

public class ResetPasswordAsyncTests : AuthServiceTestBase
{
    [Fact]
    public async Task WithValidToken_ResetsPasswordAndRevokesTokens()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Email = "test@example.com",
            Token = "valid-reset-token",
            NewPassword = "NewPassword123!"
        };

        var user = new User
        {
            Id = "user-id-123",
            Email = request.Email
        };

        UserRepositoryMock
            .Setup(x => x.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        UserRepositoryMock
            .Setup(x => x.ResetPasswordAsync(user, request.Token, request.NewPassword, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        TokenServiceMock
            .Setup(x => x.RevokeAllUserRefreshTokensAsync(user.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await AuthService.ResetPasswordAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Password has been reset successfully", result.Message);

        TokenServiceMock.Verify(
            x => x.RevokeAllUserRefreshTokensAsync(user.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task WithNonexistentEmail_ThrowsValidationException()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Email = "nonexistent@example.com",
            Token = "some-token",
            NewPassword = "NewPassword123!"
        };

        UserRepositoryMock
            .Setup(x => x.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => AuthService.ResetPasswordAsync(request)
        );

        Assert.Contains("Invalid password reset request", exception.Message);
    }

    [Fact]
    public async Task WithInvalidToken_ThrowsValidationException()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Email = "test@example.com",
            Token = "invalid-token",
            NewPassword = "NewPassword123!"
        };

        var user = new User
        {
            Id = "user-id-123",
            Email = request.Email
        };

        var errors = new[] { "Invalid token." };

        UserRepositoryMock
            .Setup(x => x.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        UserRepositoryMock
            .Setup(x => x.ResetPasswordAsync(user, request.Token, request.NewPassword, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, errors));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => AuthService.ResetPasswordAsync(request)
        );

        Assert.Contains("Invalid token", exception.Message);
    }

    [Fact]
    public async Task WithWeakPassword_ThrowsValidationException()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Email = "test@example.com",
            Token = "valid-token",
            NewPassword = "weak"
        };

        var user = new User
        {
            Id = "user-id-123",
            Email = request.Email
        };

        var errors = new[] { "Password must be at least 6 characters." };

        UserRepositoryMock
            .Setup(x => x.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        UserRepositoryMock
            .Setup(x => x.ResetPasswordAsync(user, request.Token, request.NewPassword, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, errors));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => AuthService.ResetPasswordAsync(request)
        );

        Assert.Contains("Password must be at least 6 characters", exception.Message);
    }
}