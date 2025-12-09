using AiGroupChat.Application.Exceptions;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Moq;

namespace AiGroupChat.UnitTests.Services.GroupMemberService;

public class LeaveGroupAsyncTests : GroupMemberServiceTestBase
{
    [Fact]
    public async Task WithMember_LeavesGroup()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string memberId = "member-id";

        Group group = new Group { Id = groupId, Name = "Test Group", Members = new List<GroupMember>() };
        GroupMember member = new GroupMember
        {
            UserId = memberId,
            Role = GroupRole.Member,
            User = new User { Id = memberId }
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(groupId, memberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        GroupRepositoryMock
            .Setup(x => x.RemoveMemberAsync(member, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await GroupMemberService.LeaveGroupAsync(groupId, memberId);

        // Assert
        GroupRepositoryMock.Verify(
            x => x.RemoveMemberAsync(member, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task WithAdmin_LeavesGroup()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string adminId = "admin-id";

        Group group = new Group { Id = groupId, Name = "Test Group", Members = new List<GroupMember>() };
        GroupMember admin = new GroupMember
        {
            UserId = adminId,
            Role = GroupRole.Admin,
            User = new User { Id = adminId }
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(groupId, adminId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        GroupRepositoryMock
            .Setup(x => x.RemoveMemberAsync(admin, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await GroupMemberService.LeaveGroupAsync(groupId, adminId);

        // Assert
        GroupRepositoryMock.Verify(
            x => x.RemoveMemberAsync(admin, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task WithOwner_ThrowsValidationException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string ownerId = "owner-id";

        Group group = new Group { Id = groupId, Name = "Test Group", Members = new List<GroupMember>() };
        GroupMember owner = new GroupMember
        {
            UserId = ownerId,
            Role = GroupRole.Owner,
            User = new User { Id = ownerId }
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(groupId, ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(owner);

        // Act & Assert
        ValidationException exception = await Assert.ThrowsAsync<ValidationException>(
            () => GroupMemberService.LeaveGroupAsync(groupId, ownerId)
        );

        Assert.Contains("owner", exception.Message.ToLower());
    }

    [Fact]
    public async Task WithNonMember_ThrowsAuthorizationException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string userId = "non-member-id";

        Group group = new Group { Id = groupId, Name = "Test Group", Members = new List<GroupMember>() };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(groupId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupMember?)null);

        // Act & Assert
        AuthorizationException exception = await Assert.ThrowsAsync<AuthorizationException>(
            () => GroupMemberService.LeaveGroupAsync(groupId, userId)
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
            () => GroupMemberService.LeaveGroupAsync(groupId, userId)
        );

        Assert.Contains(groupId.ToString(), exception.Message);
    }
}