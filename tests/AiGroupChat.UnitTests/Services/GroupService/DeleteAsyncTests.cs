using AiGroupChat.Application.Exceptions;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Moq;

namespace AiGroupChat.UnitTests.Services.GroupService;

public class DeleteAsyncTests : GroupServiceTestBase
{
    [Fact]
    public async Task WithValidIdAndAdmin_DeletesGroup()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var currentUserId = "user-id-123";

        var group = new Group
        {
            Id = groupId,
            Name = "Test Group",
            CreatedById = currentUserId,
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
            .Setup(x => x.GetByIdWithMembersAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, currentUserId, It.IsAny<CancellationToken>()))
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
        var groupId = Guid.NewGuid();
        var currentUserId = "user-id-123";

        GroupRepositoryMock
            .Setup(x => x.GetByIdWithMembersAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Group?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => GroupService.DeleteAsync(groupId, currentUserId)
        );

        Assert.Contains(groupId.ToString(), exception.Message);
    }

    [Fact]
    public async Task WithNonAdmin_ThrowsAuthorizationException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var currentUserId = "user-id-123";

        var group = new Group
        {
            Id = groupId,
            Name = "Test Group",
            CreatedById = "other-user",
            Members = new List<GroupMember>
            {
                new GroupMember
                {
                    UserId = currentUserId,
                    Role = GroupRole.Member,
                    User = new User { Id = currentUserId }
                }
            }
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdWithMembersAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AuthorizationException>(
            () => GroupService.DeleteAsync(groupId, currentUserId)
        );

        Assert.Contains("admin", exception.Message.ToLower());
    }
}