using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.Exceptions;
using AiGroupChat.Domain.Entities;
using Moq;

namespace AiGroupChat.UnitTests.Services.AuthService;

public class LoginAsyncTests : AuthServiceTestBase
{
    [Fact]
    public async Task WithValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        LoginRequest request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        User user = new User
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
            .Setup(x => x.IsEmailConfirmedAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        UserRepositoryMock
            .Setup(x => x.CheckPasswordAsync(user, request.Password, It.IsAny<CancellationToken>()))
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
        AuthResponse result = await AuthService.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.Equal(user.Id, result.User.Id);
        Assert.Equal(user.Email, result.User.Email);
    }

    [Fact]
    public async Task WithNonexistentEmail_ThrowsAuthenticationException()
    {
        // Arrange
        LoginRequest request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        };

        UserRepositoryMock
            .Setup(x => x.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        AuthenticationException exception = await Assert.ThrowsAsync<AuthenticationException>(
            () => AuthService.LoginAsync(request)
        );

        Assert.Equal("Invalid email or password.", exception.Message);
    }

    [Fact]
    public async Task WithUnconfirmedEmail_ThrowsAuthenticationException()
    {
        // Arrange
        LoginRequest request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        User user = new User
        {
            Id = "user-id-123",
            Email = request.Email
        };

        UserRepositoryMock
            .Setup(x => x.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        UserRepositoryMock
            .Setup(x => x.IsEmailConfirmedAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        AuthenticationException exception = await Assert.ThrowsAsync<AuthenticationException>(
            () => AuthService.LoginAsync(request)
        );

        Assert.Contains("confirm your email", exception.Message);
    }

    [Fact]
    public async Task WithInvalidPassword_ThrowsAuthenticationException()
    {
        // Arrange
        LoginRequest request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword!"
        };

        User user = new User
        {
            Id = "user-id-123",
            Email = request.Email
        };

        UserRepositoryMock
            .Setup(x => x.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        UserRepositoryMock
            .Setup(x => x.IsEmailConfirmedAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        UserRepositoryMock
            .Setup(x => x.CheckPasswordAsync(user, request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        AuthenticationException exception = await Assert.ThrowsAsync<AuthenticationException>(
            () => AuthService.LoginAsync(request)
        );

        Assert.Equal("Invalid email or password.", exception.Message);
    }
}