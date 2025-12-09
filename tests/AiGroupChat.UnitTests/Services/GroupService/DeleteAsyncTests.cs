using AiGroupChat.Application.Exceptions;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Moq;

namespace AiGroupChat.UnitTests.Services.GroupService;

public class DeleteAsyncTests : GroupServiceTestBase
{
    [Fact]
    public async Task WithValidIdAndOwner_DeletesGroup()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string currentUserId = "user-id-123";

        Group group = new Group
        {
            Id = groupId,
            Name = "Test Group",
            CreatedById = currentUserId,
            AiProviderId = DefaultAiProvider.Id,
            AiProvider = DefaultAiProvider,
            Members = new List<GroupMember>
            {
                new GroupMember
                {
                    UserId = currentUserId,
                    Role = GroupRole.Owner,
                    User = new User { Id = currentUserId }
                }
            }
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsOwnerAsync(groupId, currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        GroupRepositoryMock
            .Setup(x => x.DeleteAsync(group, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await GroupService.DeleteAsync(groupId, currentUserId);

        // Assert
        GroupRepositoryMock.Verify(
            x => x.DeleteAsync(group, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task WithNonexistentGroup_ThrowsNotFoundException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string currentUserId = "user-id-123";

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Group?)null);

        // Act & Assert
        NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
            () => GroupService.DeleteAsync(groupId, currentUserId)
        );

        Assert.Contains(groupId.ToString(), exception.Message);
    }

    [Fact]
    public async Task WithNonOwner_ThrowsAuthorizationException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string currentUserId = "user-id-123";

        Group group = new Group
        {
            Id = groupId,
            Name = "Test Group",
            CreatedById = "other-user",
            AiProviderId = DefaultAiProvider.Id,
            AiProvider = DefaultAiProvider,
            Members = new List<GroupMember>
            {
                new GroupMember
                {
                    UserId = currentUserId,
                    Role = GroupRole.Admin,
                    User = new User { Id = currentUserId }
                }
            }
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsOwnerAsync(groupId, currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        AuthorizationException exception = await Assert.ThrowsAsync<AuthorizationException>(
            () => GroupService.DeleteAsync(groupId, currentUserId)
        );

        Assert.Contains("owner", exception.Message.ToLower());
    }
}