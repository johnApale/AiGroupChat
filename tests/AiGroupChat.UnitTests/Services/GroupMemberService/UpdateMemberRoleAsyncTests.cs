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
        Guid groupId = Guid.NewGuid();
        string ownerId = "owner-id";
        string memberId = "member-id";
        UpdateMemberRoleRequest request = new UpdateMemberRoleRequest { Role = "Admin" };

        Group group = new Group { Id = groupId, Name = "Test Group", Members = new List<GroupMember>() };

        GroupMember member = new GroupMember
        {
            UserId = memberId,
            Role = GroupRole.Member,
            User = new User { Id = memberId, UserName = "member", DisplayName = "Member" }
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
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
        GroupMemberResponse result = await GroupMemberService.UpdateMemberRoleAsync(groupId, memberId, request, ownerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Admin", result.Role);
        Assert.Equal(GroupRole.Admin, member.Role);
    }

    [Fact]
    public async Task WithOwnerDemotingAdminToMember_UpdatesRole()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string ownerId = "owner-id";
        string adminId = "admin-id";
        UpdateMemberRoleRequest request = new UpdateMemberRoleRequest { Role = "Member" };

        Group group = new Group { Id = groupId, Name = "Test Group", Members = new List<GroupMember>() };

        GroupMember admin = new GroupMember
        {
            UserId = adminId,
            Role = GroupRole.Admin,
            User = new User { Id = adminId, UserName = "admin", DisplayName = "Admin" }
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
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
        GroupMemberResponse result = await GroupMemberService.UpdateMemberRoleAsync(groupId, adminId, request, ownerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Member", result.Role);
        Assert.Equal(GroupRole.Member, admin.Role);
    }

    [Fact]
    public async Task WithNonOwner_ThrowsAuthorizationException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string adminId = "admin-id";
        string memberId = "member-id";
        UpdateMemberRoleRequest request = new UpdateMemberRoleRequest { Role = "Admin" };

        Group group = new Group { Id = groupId, Name = "Test Group", Members = new List<GroupMember>() };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsOwnerAsync(groupId, adminId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        AuthorizationException exception = await Assert.ThrowsAsync<AuthorizationException>(
            () => GroupMemberService.UpdateMemberRoleAsync(groupId, memberId, request, adminId)
        );

        Assert.Contains("owner", exception.Message.ToLower());
    }

    [Fact]
    public async Task WithChangingOwnerRole_ThrowsValidationException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string ownerId = "owner-id";
        UpdateMemberRoleRequest request = new UpdateMemberRoleRequest { Role = "Admin" };

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
            .Setup(x => x.IsOwnerAsync(groupId, ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(groupId, ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(owner);

        // Act & Assert
        ValidationException exception = await Assert.ThrowsAsync<ValidationException>(
            () => GroupMemberService.UpdateMemberRoleAsync(groupId, ownerId, request, ownerId)
        );

        Assert.Contains("owner", exception.Message.ToLower());
    }

    [Fact]
    public async Task WithInvalidRole_ThrowsValidationException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string ownerId = "owner-id";
        string memberId = "member-id";
        UpdateMemberRoleRequest request = new UpdateMemberRoleRequest { Role = "InvalidRole" };

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
            .Setup(x => x.IsOwnerAsync(groupId, ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(groupId, memberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        // Act & Assert
        ValidationException exception = await Assert.ThrowsAsync<ValidationException>(
            () => GroupMemberService.UpdateMemberRoleAsync(groupId, memberId, request, ownerId)
        );

        Assert.Contains("invalid role", exception.Message.ToLower());
    }

    [Fact]
    public async Task WithOwnerAsNewRole_ThrowsValidationException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string ownerId = "owner-id";
        string memberId = "member-id";
        UpdateMemberRoleRequest request = new UpdateMemberRoleRequest { Role = "Owner" };

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
            .Setup(x => x.IsOwnerAsync(groupId, ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(groupId, memberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        // Act & Assert
        ValidationException exception = await Assert.ThrowsAsync<ValidationException>(
            () => GroupMemberService.UpdateMemberRoleAsync(groupId, memberId, request, ownerId)
        );

        Assert.Contains("invalid role", exception.Message.ToLower());
    }

    [Fact]
    public async Task WithNonexistentMember_ThrowsNotFoundException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string ownerId = "owner-id";
        string nonMemberId = "non-member-id";
        UpdateMemberRoleRequest request = new UpdateMemberRoleRequest { Role = "Admin" };

        Group group = new Group { Id = groupId, Name = "Test Group", Members = new List<GroupMember>() };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsOwnerAsync(groupId, ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(groupId, nonMemberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupMember?)null);

        // Act & Assert
        NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
            () => GroupMemberService.UpdateMemberRoleAsync(groupId, nonMemberId, request, ownerId)
        );

        Assert.Contains(nonMemberId, exception.Message);
    }
}