using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.DTOs.SignalR.PersonalChannel;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Moq;

namespace AiGroupChat.UnitTests.Services.GroupMemberService;

public class PersonalChannelNotificationTests : GroupMemberServiceTestBase
{
    private readonly Guid _groupId = Guid.NewGuid();
    private readonly string _ownerId = "owner-id";
    private readonly string _memberId = "member-id";

    [Fact]
    public async Task AddMemberAsync_SendsAddedToGroupNotification()
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

        User ownerUser = new User
        {
            Id = _ownerId,
            UserName = "owner",
            DisplayName = "Owner User"
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

        UserRepositoryMock
            .Setup(x => x.FindByIdAsync(_ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownerUser);

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
            x => x.SendAddedToGroupAsync(
                newUserId,
                It.Is<AddedToGroupEvent>(e => 
                    e.GroupId == _groupId &&
                    e.GroupName == "Test Group" &&
                    e.AddedByName == "Owner User" &&
                    e.Role == "Member"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveMemberAsync_SendsRemovedFromGroupNotification()
    {
        // Arrange
        User memberUser = new User
        {
            Id = _memberId,
            UserName = "member",
            DisplayName = "Member User"
        };

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
            JoinedAt = DateTime.UtcNow.AddDays(-1),
            User = memberUser
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
            x => x.SendRemovedFromGroupAsync(
                _memberId,
                It.Is<RemovedFromGroupEvent>(e => 
                    e.GroupId == _groupId &&
                    e.GroupName == "Test Group"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateMemberRoleAsync_SendsRoleChangedNotification()
    {
        // Arrange
        User ownerUser = new User
        {
            Id = _ownerId,
            UserName = "owner",
            DisplayName = "Owner User"
        };

        User memberUser = new User
        {
            Id = _memberId,
            UserName = "member",
            DisplayName = "Member User"
        };

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
            User = memberUser
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

        UserRepositoryMock
            .Setup(x => x.FindByIdAsync(_ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownerUser);

        // Act
        await GroupMemberService.UpdateMemberRoleAsync(_groupId, _memberId, request, _ownerId);

        // Assert
        ChatHubServiceMock.Verify(
            x => x.SendRoleChangedAsync(
                _memberId,
                It.Is<RoleChangedEvent>(e => 
                    e.GroupId == _groupId &&
                    e.GroupName == "Test Group" &&
                    e.OldRole == "Member" &&
                    e.NewRole == "Admin" &&
                    e.ChangedByName == "Owner User"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LeaveGroupAsync_DoesNotSendPersonalNotification()
    {
        // Arrange - when a user leaves voluntarily, they don't need a notification
        User memberUser = new User
        {
            Id = _memberId,
            UserName = "member",
            DisplayName = "Member User"
        };

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
            User = memberUser
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(_groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(_groupId, _memberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        // Act
        await GroupMemberService.LeaveGroupAsync(_groupId, _memberId);

        // Assert - no personal notification for voluntary leave
        ChatHubServiceMock.Verify(
            x => x.SendRemovedFromGroupAsync(
                It.IsAny<string>(),
                It.IsAny<RemovedFromGroupEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}