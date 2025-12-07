using AiGroupChat.Application.DTOs.Auth;
using Moq;

namespace AiGroupChat.UnitTests.Services.AuthService;

public class LogoutAsyncTests : AuthServiceTestBase
{
    [Fact]
    public async Task WithValidToken_RevokesTokenAndReturnsMessage()
    {
        // Arrange
        var request = new LogoutRequest
        {
            RefreshToken = "valid-refresh-token"
        };

        TokenServiceMock
            .Setup(x => x.RevokeRefreshTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await AuthService.LogoutAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Logged out successfully", result.Message);

        TokenServiceMock.Verify(
            x => x.RevokeRefreshTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task WithInvalidToken_StillReturnsSuccessMessage()
    {
        // Arrange
        var request = new LogoutRequest
        {
            RefreshToken = "nonexistent-token"
        };

        TokenServiceMock
            .Setup(x => x.RevokeRefreshTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await AuthService.LogoutAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Logged out successfully", result.Message);
    }
}