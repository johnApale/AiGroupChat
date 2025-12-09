using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.Exceptions;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Moq;

namespace AiGroupChat.UnitTests.Services.GroupMemberService;

public class GetMembersAsyncTests : GroupMemberServiceTestBase
{
    [Fact]
    public async Task WithValidMember_ReturnsMembers()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string currentUserId = "member-id";

        Group group = new Group
        {
            Id = groupId,
            Name = "Test Group",
            Members = new List<GroupMember>
            {
                new GroupMember
                {
                    UserId = "owner-id",
                    Role = GroupRole.Owner,
                    JoinedAt = DateTime.UtcNow,
                    User = new User { Id = "owner-id", UserName = "owner", DisplayName = "Owner" }
                },
                new GroupMember
                {
                    UserId = "admin-id",
                    Role = GroupRole.Admin,
                    JoinedAt = DateTime.UtcNow,
                    User = new User { Id = "admin-id", UserName = "admin", DisplayName = "Admin" }
                },
                new GroupMember
                {
                    UserId = currentUserId,
                    Role = GroupRole.Member,
                    JoinedAt = DateTime.UtcNow,
                    User = new User { Id = currentUserId, UserName = "member", DisplayName = "Member" }
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
        List<GroupMemberResponse> result = await GroupMemberService.GetMembersAsync(groupId, currentUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Contains(result, m => m.Role == "Owner");
        Assert.Contains(result, m => m.Role == "Admin");
        Assert.Contains(result, m => m.Role == "Member");
    }

    [Fact]
    public async Task WithNonMember_ThrowsAuthorizationException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string nonMemberId = "non-member-id";

        Group group = new Group
        {
            Id = groupId,
            Name = "Test Group",
            Members = new List<GroupMember>()
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsMemberAsync(groupId, nonMemberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        AuthorizationException exception = await Assert.ThrowsAsync<AuthorizationException>(
            () => GroupMemberService.GetMembersAsync(groupId, nonMemberId)
        );

        Assert.Contains("not a member", exception.Message.ToLower());
    }

    [Fact]
    public async Task WithNonexistentGroup_ThrowsNotFoundException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string userId = "user-id";

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Group?)null);

        // Act & Assert
        NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
            () => GroupMemberService.GetMembersAsync(groupId, userId)
        );

        Assert.Contains(groupId.ToString(), exception.Message);
    }
}