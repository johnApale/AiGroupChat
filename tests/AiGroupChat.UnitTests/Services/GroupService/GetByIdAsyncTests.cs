using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.Exceptions;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Moq;

namespace AiGroupChat.UnitTests.Services.GroupService;

public class GetByIdAsyncTests : GroupServiceTestBase
{
    [Fact]
    public async Task WithValidIdAndMember_ReturnsGroupResponse()
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
            .Setup(x => x.IsMemberAsync(groupId, currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        GroupResponse result = await GroupService.GetByIdAsync(groupId, currentUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(groupId, result.Id);
        Assert.Equal("Test Group", result.Name);
        Assert.Single(result.Members);
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
            () => GroupService.GetByIdAsync(groupId, currentUserId)
        );

        Assert.Contains(groupId.ToString(), exception.Message);
    }

    [Fact]
    public async Task WithNonMember_ThrowsAuthorizationException()
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
            Members = new List<GroupMember>()
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsMemberAsync(groupId, currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        AuthorizationException exception = await Assert.ThrowsAsync<AuthorizationException>(
            () => GroupService.GetByIdAsync(groupId, currentUserId)
        );

        Assert.Contains("not a member", exception.Message);
    }
}