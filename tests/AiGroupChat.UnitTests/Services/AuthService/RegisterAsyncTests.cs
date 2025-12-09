using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.Exceptions;
using AiGroupChat.Application.Models;
using AiGroupChat.Domain.Entities;
using Moq;

namespace AiGroupChat.UnitTests.Services.AuthService;

public class RegisterAsyncTests : AuthServiceTestBase
{
    [Fact]
    public async Task WithValidRequest_ReturnsSuccessMessage()
    {
        // Arrange
        RegisterRequest request = new RegisterRequest
        {
            Email = "test@example.com",
            UserName = "testuser",
            DisplayName = "Test User",
            Password = "Password123!"
        };

        UserRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        UserRepositoryMock
            .Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("confirmation-token");

        EmailServiceMock
            .Setup(x => x.SendConfirmationEmailAsync(
                request.Email,
                request.DisplayName,
                "confirmation-token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmailResult.Success("email-id"));

        // Act
        MessageResponse result = await AuthService.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Registration successful", result.Message);
    }

    [Fact]
    public async Task WithInvalidData_ThrowsValidationException()
    {
        // Arrange
        RegisterRequest request = new RegisterRequest
        {
            Email = "test@example.com",
            UserName = "testuser",
            DisplayName = "Test User",
            Password = "weak"
        };

        string[] errors = new[] { "Password must be at least 6 characters." };

        UserRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, errors));

        // Act & Assert
        ValidationException exception = await Assert.ThrowsAsync<ValidationException>(
            () => AuthService.RegisterAsync(request)
        );

        Assert.Contains("Password must be at least 6 characters", exception.Message);
    }

    [Fact]
    public async Task SendsConfirmationEmail()
    {
        // Arrange
        RegisterRequest request = new RegisterRequest
        {
            Email = "test@example.com",
            UserName = "testuser",
            DisplayName = "Test User",
            Password = "Password123!"
        };

        UserRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        UserRepositoryMock
            .Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("confirmation-token");

        EmailServiceMock
            .Setup(x => x.SendConfirmationEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmailResult.Success("email-id"));

        // Act
        await AuthService.RegisterAsync(request);

        // Assert
        EmailServiceMock.Verify(
            x => x.SendConfirmationEmailAsync(
                request.Email,
                request.DisplayName,
                "confirmation-token",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}