using AiGroupChat.Application.Exceptions;
using AiGroupChat.Domain.Entities;
using Moq;

namespace AiGroupChat.UnitTests.Services.UserService;

public class GetByIdAsyncTests : UserServiceTestBase
{
    [Fact]
    public async Task WithValidId_ReturnsUserResponse()
    {
        // Arrange
        var user = new User
        {
            Id = "user-id-123",
            Email = "test@example.com",
            UserName = "testuser",
            DisplayName = "Test User",
            CreatedAt = DateTime.UtcNow
        };

        UserRepositoryMock
            .Setup(x => x.FindByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await UserService.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.UserName, result.UserName);
        Assert.Equal(user.DisplayName, result.DisplayName);
        Assert.Equal(user.CreatedAt, result.CreatedAt);
    }

    [Fact]
    public async Task WithNonexistentId_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "nonexistent-id";

        UserRepositoryMock
            .Setup(x => x.FindByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => UserService.GetByIdAsync(userId)
        );

        Assert.Contains(userId, exception.Message);
    }
}