using AiGroupChat.Application.DTOs.Users;
using AiGroupChat.Application.Exceptions;
using AiGroupChat.Domain.Entities;
using Moq;

namespace AiGroupChat.UnitTests.Services.UserService;

public class GetCurrentUserAsyncTests : UserServiceTestBase
{
    [Fact]
    public async Task WithValidCurrentUserId_ReturnsUserResponse()
    {
        // Arrange
        User user = new User
        {
            Id = "current-user-id",
            Email = "current@example.com",
            UserName = "currentuser",
            DisplayName = "Current User",
            CreatedAt = DateTime.UtcNow
        };

        UserRepositoryMock
            .Setup(x => x.FindByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        UserResponse result = await UserService.GetCurrentUserAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.DisplayName, result.DisplayName);
    }

    [Fact]
    public async Task WithInvalidCurrentUserId_ThrowsNotFoundException()
    {
        // Arrange
        string userId = "deleted-user-id";

        UserRepositoryMock
            .Setup(x => x.FindByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => UserService.GetCurrentUserAsync(userId)
        );
    }
}