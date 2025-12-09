using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.Exceptions;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Moq;

namespace AiGroupChat.UnitTests.Services.GroupService;

public class UpdateAsyncTests : GroupServiceTestBase
{
    [Fact]
    public async Task WithValidRequestAndAdmin_UpdatesAndReturnsGroup()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string currentUserId = "user-id-123";
        UpdateGroupRequest request = new UpdateGroupRequest
        {
            Name = "Updated Group Name"
        };

        Group group = new Group
        {
            Id = groupId,
            Name = "Original Name",
            CreatedById = currentUserId,
            AiProviderId = DefaultAiProvider.Id,
            AiProvider = DefaultAiProvider,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Members = new List<GroupMember>
            {
                new GroupMember
                {
                    UserId = currentUserId,
                    Role = GroupRole.Admin,
                    JoinedAt = DateTime.UtcNow,
                    User = new User { Id = currentUserId, UserName = "testuser", DisplayName = "Test User" }
                }
            }
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        GroupRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Group>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Group g, CancellationToken _) => g);

        // Act
        GroupResponse result = await GroupService.UpdateAsync(groupId, request, currentUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Name, result.Name);
    }

    [Fact]
    public async Task WithNonexistentGroup_ThrowsNotFoundException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string currentUserId = "user-id-123";
        UpdateGroupRequest request = new UpdateGroupRequest { Name = "Updated Name" };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Group?)null);

        // Act & Assert
        NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
            () => GroupService.UpdateAsync(groupId, request, currentUserId)
        );

        Assert.Contains(groupId.ToString(), exception.Message);
    }

    [Fact]
    public async Task WithNonAdmin_ThrowsAuthorizationException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string currentUserId = "user-id-123";
        UpdateGroupRequest request = new UpdateGroupRequest { Name = "Updated Name" };

        Group group = new Group
        {
            Id = groupId,
            Name = "Original Name",
            CreatedById = "other-user",
            AiProviderId = DefaultAiProvider.Id,
            AiProvider = DefaultAiProvider,
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
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        AuthorizationException exception = await Assert.ThrowsAsync<AuthorizationException>(
            () => GroupService.UpdateAsync(groupId, request, currentUserId)
        );

        Assert.Contains("admin", exception.Message.ToLower());
    }
}