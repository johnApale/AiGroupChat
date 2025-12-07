using AiGroupChat.Application.Exceptions;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Moq;

namespace AiGroupChat.UnitTests.Services.GroupMemberService;

public class RemoveMemberAsyncTests : GroupMemberServiceTestBase
{
    [Fact]
    public async Task WithOwnerRemovingMember_RemovesMember()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var ownerId = "owner-id";
        var memberId = "member-id";

        var group = new Group { Id = groupId, Name = "Test Group", Members = new List<GroupMember>() };
        var member = new GroupMember
        {
            UserId = memberId,
            Role = GroupRole.Member,
            User = new User { Id = memberId }
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdWithMembersAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(groupId, memberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        GroupRepositoryMock
            .Setup(x => x.IsOwnerAsync(groupId, ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        GroupRepositoryMock
            .Setup(x => x.RemoveMemberAsync(member, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await GroupMemberService.RemoveMemberAsync(groupId, memberId, ownerId);

        // Assert
        GroupRepositoryMock.Verify(
            x => x.RemoveMemberAsync(member, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task WithOwnerRemovingAdmin_RemovesAdmin()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var ownerId = "owner-id";
        var adminId = "admin-id";

        var group = new Group { Id = groupId, Name = "Test Group", Members = new List<GroupMember>() };
        var admin = new GroupMember
        {
            UserId = adminId,
            Role = GroupRole.Admin,
            User = new User { Id = adminId }
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdWithMembersAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(groupId, adminId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        GroupRepositoryMock
            .Setup(x => x.IsOwnerAsync(groupId, ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        GroupRepositoryMock
            .Setup(x => x.RemoveMemberAsync(admin, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await GroupMemberService.RemoveMemberAsync(groupId, adminId, ownerId);

        // Assert
        GroupRepositoryMock.Verify(
            x => x.RemoveMemberAsync(admin, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task WithAdminRemovingMember_RemovesMember()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var adminId = "admin-id";
        var memberId = "member-id";

        var group = new Group { Id = groupId, Name = "Test Group", Members = new List<GroupMember>() };
        var member = new GroupMember
        {
            UserId = memberId,
            Role = GroupRole.Member,
            User = new User { Id = memberId }
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdWithMembersAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(groupId, memberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        GroupRepositoryMock
            .Setup(x => x.IsOwnerAsync(groupId, adminId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, adminId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        GroupRepositoryMock
            .Setup(x => x.RemoveMemberAsync(member, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await GroupMemberService.RemoveMemberAsync(groupId, memberId, adminId);

        // Assert
        GroupRepositoryMock.Verify(
            x => x.RemoveMemberAsync(member, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task WithAdminRemovingAdmin_ThrowsAuthorizationException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var adminId = "admin-id";
        var otherAdminId = "other-admin-id";

        var group = new Group { Id = groupId, Name = "Test Group", Members = new List<GroupMember>() };
        var otherAdmin = new GroupMember
        {
            UserId = otherAdminId,
            Role = GroupRole.Admin,
            User = new User { Id = otherAdminId }
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdWithMembersAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(groupId, otherAdminId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherAdmin);

        GroupRepositoryMock
            .Setup(x => x.IsOwnerAsync(groupId, adminId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, adminId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AuthorizationException>(
            () => GroupMemberService.RemoveMemberAsync(groupId, otherAdminId, adminId)
        );

        Assert.Contains("owner", exception.Message.ToLower());
    }

    [Fact]
    public async Task WithRemovingOwner_ThrowsValidationException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var ownerId = "owner-id";
        var adminId = "admin-id";

        var group = new Group { Id = groupId, Name = "Test Group", Members = new List<GroupMember>() };
        var owner = new GroupMember
        {
            UserId = ownerId,
            Role = GroupRole.Owner,
            User = new User { Id = ownerId }
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdWithMembersAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(groupId, ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(owner);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => GroupMemberService.RemoveMemberAsync(groupId, ownerId, adminId)
        );

        Assert.Contains("owner", exception.Message.ToLower());
    }

    [Fact]
    public async Task WithNonAdmin_ThrowsAuthorizationException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var memberId = "member-id";
        var otherMemberId = "other-member-id";

        var group = new Group { Id = groupId, Name = "Test Group", Members = new List<GroupMember>() };
        var otherMember = new GroupMember
        {
            UserId = otherMemberId,
            Role = GroupRole.Member,
            User = new User { Id = otherMemberId }
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdWithMembersAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(groupId, otherMemberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherMember);

        GroupRepositoryMock
            .Setup(x => x.IsOwnerAsync(groupId, memberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, memberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AuthorizationException>(
            () => GroupMemberService.RemoveMemberAsync(groupId, otherMemberId, memberId)
        );

        Assert.Contains("admin", exception.Message.ToLower());
    }
}