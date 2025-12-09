using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Moq;

namespace AiGroupChat.UnitTests.Services.GroupService;

public class GetMyGroupsAsyncTests : GroupServiceTestBase
{
    [Fact]
    public async Task WithGroups_ReturnsGroupList()
    {
        // Arrange
        string currentUserId = "user-id-123";
        List<Group> groups = new List<Group>
        {
            new Group
            {
                Id = Guid.NewGuid(),
                Name = "Group 1",
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
                        Role = GroupRole.Owner,
                        JoinedAt = DateTime.UtcNow,
                        User = new User { Id = currentUserId, UserName = "testuser", DisplayName = "Test User" }
                    }
                }
            },
            new Group
            {
                Id = Guid.NewGuid(),
                Name = "Group 2",
                CreatedById = "other-user",
                AiProviderId = DefaultAiProvider.Id,
                AiProvider = DefaultAiProvider,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Members = new List<GroupMember>
                {
                    new GroupMember
                    {
                        UserId = currentUserId,
                        Role = GroupRole.Member,
                        JoinedAt = DateTime.UtcNow,
                        User = new User { Id = currentUserId, UserName = "testuser", DisplayName = "Test User" }
                    }
                }
            }
        };

        GroupRepositoryMock
            .Setup(x => x.GetGroupsByUserIdAsync(currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(groups);

        // Act
        List<GroupResponse> result = await GroupService.GetMyGroupsAsync(currentUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Group 1", result[0].Name);
        Assert.Equal("Group 2", result[1].Name);
    }

    [Fact]
    public async Task WithNoGroups_ReturnsEmptyList()
    {
        // Arrange
        string currentUserId = "user-id-123";

        GroupRepositoryMock
            .Setup(x => x.GetGroupsByUserIdAsync(currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Group>());

        // Act
        List<GroupResponse> result = await GroupService.GetMyGroupsAsync(currentUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}