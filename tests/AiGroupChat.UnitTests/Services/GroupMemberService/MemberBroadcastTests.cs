using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Moq;

namespace AiGroupChat.UnitTests.Services.GroupMemberService;

public class MemberBroadcastTests : GroupMemberServiceTestBase
{
    private readonly Guid _groupId = Guid.NewGuid();
    private readonly string _ownerId = "owner-id";
    private readonly string _memberId = "member-id";

    [Fact]
    public async Task AddMemberAsync_BroadcastsMemberAdded()
    {
        // Arrange
        string newUserId = "new-user-id";
        AddMemberRequest request = new AddMemberRequest { UserId = newUserId };

        Group group = new Group
        {
            Id = _groupId,
            Name = "Test Group",
            Members = new List<GroupMember>()
        };

        User newUser = new User
        {
            Id = newUserId,
            UserName = "newuser",
            DisplayName = "New User"
        };

        GroupMember addedMember = new GroupMember
        {
            UserId = newUserId,
            Role = GroupRole.Member,
            JoinedAt = DateTime.UtcNow,
            User = newUser
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(_groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(_groupId, _ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        UserRepositoryMock
            .Setup(x => x.FindByIdAsync(newUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newUser);

        GroupRepositoryMock
            .SetupSequence(x => x.GetMemberAsync(_groupId, newUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupMember?)null)
            .ReturnsAsync(addedMember);

        GroupRepositoryMock
            .Setup(x => x.AddMemberAsync(It.IsAny<GroupMember>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupMember m, CancellationToken _) => m);

        // Act
        await GroupMemberService.AddMemberAsync(_groupId, request, _ownerId);

        // Assert
        ChatHubServiceMock.Verify(
            x => x.BroadcastMemberAddedAsync(
                _groupId,
                It.Is<GroupMemberResponse>(m => m.UserId == newUserId && m.Role == "Member"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveMemberAsync_BroadcastsMemberRemoved()
    {
        // Arrange
        Group group = new Group
        {
            Id = _groupId,
            Name = "Test Group",
            Members = new List<GroupMember>()
        };

        GroupMember memberToRemove = new GroupMember
        {
            UserId = _memberId,
            Role = GroupRole.Member,
            JoinedAt = DateTime.UtcNow.AddDays(-1)
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(_groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(_groupId, _memberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(memberToRemove);

        GroupRepositoryMock
            .Setup(x => x.IsOwnerAsync(_groupId, _ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(_groupId, _ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await GroupMemberService.RemoveMemberAsync(_groupId, _memberId, _ownerId);

        // Assert
        ChatHubServiceMock.Verify(
            x => x.BroadcastMemberRemovedAsync(_groupId, _memberId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LeaveGroupAsync_BroadcastsMemberRemoved()
    {
        // Arrange
        Group group = new Group
        {
            Id = _groupId,
            Name = "Test Group",
            Members = new List<GroupMember>()
        };

        GroupMember member = new GroupMember
        {
            UserId = _memberId,
            Role = GroupRole.Member,
            JoinedAt = DateTime.UtcNow.AddDays(-1)
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(_groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(_groupId, _memberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        // Act
        await GroupMemberService.LeaveGroupAsync(_groupId, _memberId);

        // Assert
        ChatHubServiceMock.Verify(
            x => x.BroadcastMemberRemovedAsync(_groupId, _memberId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateMemberRoleAsync_BroadcastsMemberRoleChanged()
    {
        // Arrange
        Group group = new Group
        {
            Id = _groupId,
            Name = "Test Group",
            Members = new List<GroupMember>()
        };

        GroupMember member = new GroupMember
        {
            UserId = _memberId,
            Role = GroupRole.Member,
            JoinedAt = DateTime.UtcNow.AddDays(-1),
            User = new User { Id = _memberId, UserName = "member", DisplayName = "Member" }
        };

        UpdateMemberRoleRequest request = new UpdateMemberRoleRequest { Role = "Admin" };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(_groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsOwnerAsync(_groupId, _ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(_groupId, _memberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        // Act
        await GroupMemberService.UpdateMemberRoleAsync(_groupId, _memberId, request, _ownerId);

        // Assert
        ChatHubServiceMock.Verify(
            x => x.BroadcastMemberRoleChangedAsync(_groupId, _memberId, "Admin", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task TransferOwnershipAsync_BroadcastsRoleChangesForBothUsers()
    {
        // Arrange
        string newOwnerId = "new-owner-id";
        Group group = new Group
        {
            Id = _groupId,
            Name = "Test Group",
            Members = new List<GroupMember>()
        };

        GroupMember currentOwnerMember = new GroupMember
        {
            UserId = _ownerId,
            Role = GroupRole.Owner,
            JoinedAt = DateTime.UtcNow.AddDays(-7)
        };

        GroupMember newOwnerMember = new GroupMember
        {
            UserId = newOwnerId,
            Role = GroupRole.Admin,
            JoinedAt = DateTime.UtcNow.AddDays(-1),
            User = new User { Id = newOwnerId, UserName = "newowner", DisplayName = "New Owner" }
        };

        TransferOwnershipRequest request = new TransferOwnershipRequest { NewOwnerUserId = newOwnerId };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(_groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(_groupId, _ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentOwnerMember);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(_groupId, newOwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newOwnerMember);

        // Act
        await GroupMemberService.TransferOwnershipAsync(_groupId, request, _ownerId);

        // Assert - both role changes should be broadcast
        ChatHubServiceMock.Verify(
            x => x.BroadcastMemberRoleChangedAsync(_groupId, _ownerId, "Admin", It.IsAny<CancellationToken>()),
            Times.Once);

        ChatHubServiceMock.Verify(
            x => x.BroadcastMemberRoleChangedAsync(_groupId, newOwnerId, "Owner", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
