using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.Exceptions;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Moq;

namespace AiGroupChat.UnitTests.Services.GroupMemberService;

public class UpdateMemberRoleAsyncTests : GroupMemberServiceTestBase
{
    [Fact]
    public async Task WithOwnerPromotingMemberToAdmin_UpdatesRole()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var ownerId = "owner-id";
        var memberId = "member-id";
        var request = new UpdateMemberRoleRequest { Role = "Admin" };

        var group = new Group { Id = groupId, Name = "Test Group", Members = new List<GroupMember>() };

        var member = new GroupMember
        {
            UserId = memberId,
            Role = GroupRole.Member,
            User = new User { Id = memberId, UserName = "member", DisplayName = "Member" }
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdWithMembersAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsOwnerAsync(groupId, ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(groupId, memberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        GroupRepositoryMock
            .Setup(x => x.UpdateMemberAsync(It.IsAny<GroupMember>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupMember m, CancellationToken _) => m);

        // Act
        var result = await GroupMemberService.UpdateMemberRoleAsync(groupId, memberId, request, ownerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Admin", result.Role);
        Assert.Equal(GroupRole.Admin, member.Role);
    }

    [Fact]
    public async Task WithOwnerDemotingAdminToMember_UpdatesRole()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var ownerId = "owner-id";
        var adminId = "admin-id";
        var request = new UpdateMemberRoleRequest { Role = "Member" };

        var group = new Group { Id = groupId, Name = "Test Group", Members = new List<GroupMember>() };

        var admin = new GroupMember
        {
            UserId = adminId,
            Role = GroupRole.Admin,
            User = new User { Id = adminId, UserName = "admin", DisplayName = "Admin" }
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdWithMembersAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsOwnerAsync(groupId, ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(groupId, adminId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        GroupRepositoryMock
            .Setup(x => x.UpdateMemberAsync(It.IsAny<GroupMember>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupMember m, CancellationToken _) => m);

        // Act
        var result = await GroupMemberService.UpdateMemberRoleAsync(groupId, adminId, request, ownerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Member", result.Role);
        Assert.Equal(GroupRole.Member, admin.Role);
    }

    [Fact]
    public async Task WithNonOwner_ThrowsAuthorizationException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var adminId = "admin-id";
        var memberId = "member-id";
        var request = new UpdateMemberRoleRequest { Role = "Admin" };

        var group = new Group { Id = groupId, Name = "Test Group", Members = new List<GroupMember>() };

        GroupRepositoryMock
            .Setup(x => x.GetByIdWithMembersAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsOwnerAsync(groupId, adminId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AuthorizationException>(
            () => GroupMemberService.UpdateMemberRoleAsync(groupId, memberId, request, adminId)
        );

        Assert.Contains("owner", exception.Message.ToLower());
    }

    [Fact]
    public async Task WithChangingOwnerRole_ThrowsValidationException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var ownerId = "owner-id";
        var request = new UpdateMemberRoleRequest { Role = "Admin" };

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
            .Setup(x => x.IsOwnerAsync(groupId, ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(groupId, ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(owner);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => GroupMemberService.UpdateMemberRoleAsync(groupId, ownerId, request, ownerId)
        );

        Assert.Contains("owner", exception.Message.ToLower());
    }

    [Fact]
    public async Task WithInvalidRole_ThrowsValidationException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var ownerId = "owner-id";
        var memberId = "member-id";
        var request = new UpdateMemberRoleRequest { Role = "InvalidRole" };

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
            .Setup(x => x.IsOwnerAsync(groupId, ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(groupId, memberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => GroupMemberService.UpdateMemberRoleAsync(groupId, memberId, request, ownerId)
        );

        Assert.Contains("invalid role", exception.Message.ToLower());
    }

    [Fact]
    public async Task WithOwnerAsNewRole_ThrowsValidationException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var ownerId = "owner-id";
        var memberId = "member-id";
        var request = new UpdateMemberRoleRequest { Role = "Owner" };

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
            .Setup(x => x.IsOwnerAsync(groupId, ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(groupId, memberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => GroupMemberService.UpdateMemberRoleAsync(groupId, memberId, request, ownerId)
        );

        Assert.Contains("invalid role", exception.Message.ToLower());
    }

    [Fact]
    public async Task WithNonexistentMember_ThrowsNotFoundException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var ownerId = "owner-id";
        var nonMemberId = "non-member-id";
        var request = new UpdateMemberRoleRequest { Role = "Admin" };

        var group = new Group { Id = groupId, Name = "Test Group", Members = new List<GroupMember>() };

        GroupRepositoryMock
            .Setup(x => x.GetByIdWithMembersAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsOwnerAsync(groupId, ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(groupId, nonMemberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupMember?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => GroupMemberService.UpdateMemberRoleAsync(groupId, nonMemberId, request, ownerId)
        );

        Assert.Contains(nonMemberId, exception.Message);
    }
}