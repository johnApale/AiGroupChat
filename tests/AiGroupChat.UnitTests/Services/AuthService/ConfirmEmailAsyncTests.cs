using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.Exceptions;
using AiGroupChat.Domain.Entities;
using Moq;

namespace AiGroupChat.UnitTests.Services.AuthService;

public class ConfirmEmailAsyncTests : AuthServiceTestBase
{
    [Fact]
    public async Task WithValidToken_ReturnsAuthResponse()
    {
        // Arrange
        var request = new ConfirmEmailRequest
        {
            Email = "test@example.com",
            Token = "valid-confirmation-token"
        };

        var user = new User
        {
            Id = "user-id-123",
            Email = request.Email,
            UserName = "testuser",
            DisplayName = "Test User"
        };

        UserRepositoryMock
            .Setup(x => x.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        UserRepositoryMock
            .Setup(x => x.ConfirmEmailAsync(user, request.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        TokenServiceMock
            .Setup(x => x.GenerateAccessToken(user))
            .Returns("access-token");

        TokenServiceMock
            .Setup(x => x.GenerateRefreshTokenAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync("refresh-token");

        TokenServiceMock
            .Setup(x => x.GetAccessTokenExpiration())
            .Returns(DateTime.UtcNow.AddMinutes(15));

        // Act
        var result = await AuthService.ConfirmEmailAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.Equal(user.Id, result.User.Id);
    }

    [Fact]
    public async Task WithNonexistentEmail_ThrowsValidationException()
    {
        // Arrange
        var request = new ConfirmEmailRequest
        {
            Email = "nonexistent@example.com",
            Token = "some-token"
        };

        UserRepositoryMock
            .Setup(x => x.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => AuthService.ConfirmEmailAsync(request)
        );

        Assert.Contains("Invalid confirmation request", exception.Message);
    }

    [Fact]
    public async Task WithInvalidToken_ThrowsValidationException()
    {
        // Arrange
        var request = new ConfirmEmailRequest
        {
            Email = "test@example.com",
            Token = "invalid-token"
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
            .Setup(x => x.ConfirmEmailAsync(user, request.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => AuthService.ConfirmEmailAsync(request)
        );

        Assert.Contains("Invalid or expired confirmation token", exception.Message);
    }
}