using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.Exceptions;
using AiGroupChat.Domain.Entities;
using Moq;

namespace AiGroupChat.UnitTests.Services.AuthService;

public class RefreshTokenAsyncTests : AuthServiceTestBase
{
    [Fact]
    public async Task WithValidToken_ReturnsNewAuthResponse()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "valid-refresh-token"
        };

        var user = new User
        {
            Id = "user-id-123",
            Email = "test@example.com",
            UserName = "testuser",
            DisplayName = "Test User"
        };

        TokenServiceMock
            .Setup(x => x.ValidateRefreshTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user.Id);

        UserRepositoryMock
            .Setup(x => x.FindByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        TokenServiceMock
            .Setup(x => x.RevokeRefreshTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        TokenServiceMock
            .Setup(x => x.GenerateAccessToken(user))
            .Returns("new-access-token");

        TokenServiceMock
            .Setup(x => x.GenerateRefreshTokenAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync("new-refresh-token");

        TokenServiceMock
            .Setup(x => x.GetAccessTokenExpiration())
            .Returns(DateTime.UtcNow.AddMinutes(15));

        // Act
        var result = await AuthService.RefreshTokenAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("new-access-token", result.AccessToken);
        Assert.Equal("new-refresh-token", result.RefreshToken);
        Assert.Equal(user.Id, result.User.Id);
    }

    [Fact]
    public async Task WithValidToken_RevokesOldToken()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "valid-refresh-token"
        };

        var user = new User
        {
            Id = "user-id-123",
            Email = "test@example.com",
            UserName = "testuser",
            DisplayName = "Test User"
        };

        TokenServiceMock
            .Setup(x => x.ValidateRefreshTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user.Id);

        UserRepositoryMock
            .Setup(x => x.FindByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        TokenServiceMock
            .Setup(x => x.RevokeRefreshTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        TokenServiceMock
            .Setup(x => x.GenerateAccessToken(user))
            .Returns("new-access-token");

        TokenServiceMock
            .Setup(x => x.GenerateRefreshTokenAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync("new-refresh-token");

        TokenServiceMock
            .Setup(x => x.GetAccessTokenExpiration())
            .Returns(DateTime.UtcNow.AddMinutes(15));

        // Act
        await AuthService.RefreshTokenAsync(request);

        // Assert
        TokenServiceMock.Verify(
            x => x.RevokeRefreshTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task WithInvalidToken_ThrowsAuthenticationException()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "invalid-refresh-token"
        };

        TokenServiceMock
            .Setup(x => x.ValidateRefreshTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AuthenticationException>(
            () => AuthService.RefreshTokenAsync(request)
        );

        Assert.Contains("Invalid or expired refresh token", exception.Message);
    }

    [Fact]
    public async Task WithDeletedUser_ThrowsAuthenticationException()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "valid-refresh-token"
        };

        TokenServiceMock
            .Setup(x => x.ValidateRefreshTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync("deleted-user-id");

        UserRepositoryMock
            .Setup(x => x.FindByIdAsync("deleted-user-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AuthenticationException>(
            () => AuthService.RefreshTokenAsync(request)
        );

        Assert.Contains("User not found", exception.Message);
    }
}