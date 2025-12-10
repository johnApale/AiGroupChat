using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.DTOs.SignalR.GroupChannel;
using AiGroupChat.Application.DTOs.SignalR.PersonalChannel;
using AiGroupChat.IntegrationTests.Helpers;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Hubs.ChatHub;

/// <summary>
/// Integration tests for member-related SignalR events
/// </summary>
[Collection("SignalR")]
public class MemberEventTests : SignalRIntegrationTestBase
{
    public MemberEventTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AddMember_JoinedMembersReceiveMemberJoined()
    {
        // Arrange
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "addowner@test.com",
            userName: "addowner");

        GroupResponse group = await Groups.CreateGroupAsync("Add Member Group");

        Auth.ClearAuthToken();
        AuthResponse newMember = await Auth.CreateAuthenticatedUserAsync(
            email: "newmember@test.com",
            userName: "newmember",
            displayName: "New Member");

        // Owner joins SignalR group
        SignalRHelper ownerConnection = await CreateSignalRConnectionAsync(owner.AccessToken);
        await ownerConnection.JoinGroupAsync(group.Id);

        // Act - Add new member
        Auth.SetAuthToken(owner.AccessToken);
        await Members.AddMemberAsync(group.Id, newMember.User.Id);

        // Assert - Owner should receive MemberJoined event
        MemberJoinedEvent joinedEvent = await ownerConnection.WaitForMemberJoinedEventAsync(
            e => e.UserId == newMember.User.Id);

        Assert.Equal(group.Id, joinedEvent.GroupId);
        Assert.Equal("newmember", joinedEvent.UserName);
        Assert.Equal("New Member", joinedEvent.DisplayName);
    }

    [Fact]
    public async Task AddMember_NewMemberReceivesAddedToGroup()
    {
        // Arrange
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "notifyowner@test.com",
            userName: "notifyowner");

        GroupResponse group = await Groups.CreateGroupAsync("Notify Group");

        Auth.ClearAuthToken();
        AuthResponse newMember = await Auth.CreateAuthenticatedUserAsync(
            email: "notified@test.com",
            userName: "notified");

        // New member connects (auto-joins personal channel)
        SignalRHelper newMemberConnection = await CreateSignalRConnectionAsync(newMember.AccessToken);

        // Act - Owner adds new member
        Auth.SetAuthToken(owner.AccessToken);
        await Members.AddMemberAsync(group.Id, newMember.User.Id);

        // Assert - New member should receive AddedToGroup on personal channel
        AddedToGroupEvent addedEvent = await newMemberConnection.WaitForAddedToGroupEventAsync(
            e => e.GroupId == group.Id);

        Assert.Equal(group.Id, addedEvent.GroupId);
    }

    [Fact]
    public async Task RemoveMember_JoinedMembersReceiveMemberLeft()
    {
        // Arrange
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "removeowner@test.com",
            userName: "removeowner");

        GroupResponse group = await Groups.CreateGroupAsync("Remove Member Group");

        Auth.ClearAuthToken();
        AuthResponse memberToRemove = await Auth.CreateAuthenticatedUserAsync(
            email: "toremove@test.com",
            userName: "toremove");

        Auth.SetAuthToken(owner.AccessToken);
        await Members.AddMemberAsync(group.Id, memberToRemove.User.Id);

        // Owner joins SignalR group
        SignalRHelper ownerConnection = await CreateSignalRConnectionAsync(owner.AccessToken);
        await ownerConnection.JoinGroupAsync(group.Id);

        // Act - Remove member
        await Members.RemoveMemberAsync(group.Id, memberToRemove.User.Id);

        // Assert - Owner should receive MemberLeft event
        MemberLeftEvent leftEvent = await ownerConnection.WaitForMemberLeftEventAsync(
            e => e.UserId == memberToRemove.User.Id);

        Assert.Equal(group.Id, leftEvent.GroupId);
    }

    [Fact]
    public async Task RemoveMember_RemovedMemberReceivesRemovedFromGroup()
    {
        // Arrange
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "kickowner@test.com",
            userName: "kickowner");

        GroupResponse group = await Groups.CreateGroupAsync("Kick Group");

        Auth.ClearAuthToken();
        AuthResponse memberToKick = await Auth.CreateAuthenticatedUserAsync(
            email: "kicked@test.com",
            userName: "kicked");

        Auth.SetAuthToken(owner.AccessToken);
        await Members.AddMemberAsync(group.Id, memberToKick.User.Id);

        // Kicked member connects (auto-joins personal channel)
        SignalRHelper kickedConnection = await CreateSignalRConnectionAsync(memberToKick.AccessToken);

        // Act - Owner removes member
        Auth.SetAuthToken(owner.AccessToken);
        await Members.RemoveMemberAsync(group.Id, memberToKick.User.Id);

        // Assert - Removed member receives RemovedFromGroup on personal channel
        RemovedFromGroupEvent removedEvent = await kickedConnection.WaitForRemovedFromGroupEventAsync(
            e => e.GroupId == group.Id);

        Assert.Equal(group.Id, removedEvent.GroupId);
    }

    [Fact]
    public async Task UpdateRole_JoinedMembersReceiveMemberRoleChanged()
    {
        // Arrange
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "roleowner@test.com",
            userName: "roleowner");

        GroupResponse group = await Groups.CreateGroupAsync("Role Change Group");

        Auth.ClearAuthToken();
        AuthResponse member = await Auth.CreateAuthenticatedUserAsync(
            email: "promoted@test.com",
            userName: "promoted");

        Auth.SetAuthToken(owner.AccessToken);
        await Members.AddMemberAsync(group.Id, member.User.Id);

        // Owner joins SignalR group
        SignalRHelper ownerConnection = await CreateSignalRConnectionAsync(owner.AccessToken);
        await ownerConnection.JoinGroupAsync(group.Id);

        // Act - Change member's role to Admin
        await Members.UpdateMemberRoleAsync(group.Id, member.User.Id, "Admin");

        // Assert - Owner should receive MemberRoleChanged event
        MemberRoleChangedEvent roleEvent = await ownerConnection.WaitForMemberRoleChangedEventAsync(
            e => e.UserId == member.User.Id);

        Assert.Equal(group.Id, roleEvent.GroupId);
        Assert.Equal("Admin", roleEvent.NewRole);
    }

    [Fact]
    public async Task UpdateRole_AffectedMemberReceivesRoleChanged()
    {
        // Arrange
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "promoteowner@test.com",
            userName: "promoteowner");

        GroupResponse group = await Groups.CreateGroupAsync("Promote Group");

        Auth.ClearAuthToken();
        AuthResponse member = await Auth.CreateAuthenticatedUserAsync(
            email: "getpromoted@test.com",
            userName: "getpromoted");

        Auth.SetAuthToken(owner.AccessToken);
        await Members.AddMemberAsync(group.Id, member.User.Id);

        // Member connects (auto-joins personal channel)
        SignalRHelper memberConnection = await CreateSignalRConnectionAsync(member.AccessToken);

        // Act - Owner promotes member
        Auth.SetAuthToken(owner.AccessToken);
        await Members.UpdateMemberRoleAsync(group.Id, member.User.Id, "Admin");

        // Assert - Member receives RoleChanged on personal channel
        RoleChangedEvent roleEvent = await memberConnection.WaitForRoleChangedEventAsync(
            e => e.GroupId == group.Id);

        Assert.Equal("Admin", roleEvent.NewRole);
    }

    [Fact]
    public async Task LeaveGroup_MembersReceiveMemberLeft()
    {
        // Arrange
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "stayowner@test.com",
            userName: "stayowner");

        GroupResponse group = await Groups.CreateGroupAsync("Leave Group");

        Auth.ClearAuthToken();
        AuthResponse leaver = await Auth.CreateAuthenticatedUserAsync(
            email: "leaver@test.com",
            userName: "leaver");

        Auth.SetAuthToken(owner.AccessToken);
        await Members.AddMemberAsync(group.Id, leaver.User.Id);

        // Owner joins SignalR group
        SignalRHelper ownerConnection = await CreateSignalRConnectionAsync(owner.AccessToken);
        await ownerConnection.JoinGroupAsync(group.Id);

        // Act - Member leaves group
        Auth.SetAuthToken(leaver.AccessToken);
        await Members.LeaveGroupAsync(group.Id);

        // Assert - Owner should receive MemberLeft event
        MemberLeftEvent leftEvent = await ownerConnection.WaitForMemberLeftEventAsync(
            e => e.UserId == leaver.User.Id);

        Assert.Equal(group.Id, leftEvent.GroupId);
    }

    [Fact]
    public async Task TransferOwnership_BroadcastsRoleChanges()
    {
        // Arrange
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "transferowner@test.com",
            userName: "transferowner");

        GroupResponse group = await Groups.CreateGroupAsync("Transfer Group");

        Auth.ClearAuthToken();
        AuthResponse newOwner = await Auth.CreateAuthenticatedUserAsync(
            email: "newowner@test.com",
            userName: "newowner");

        Auth.SetAuthToken(owner.AccessToken);
        await Members.AddMemberAsync(group.Id, newOwner.User.Id);

        // New owner connects to receive personal notifications
        SignalRHelper newOwnerConnection = await CreateSignalRConnectionAsync(newOwner.AccessToken);

        // Act - Transfer ownership
        Auth.SetAuthToken(owner.AccessToken);
        await Members.TransferOwnershipAsync(group.Id, newOwner.User.Id);

        // Assert - New owner should receive RoleChanged to Owner
        RoleChangedEvent roleEvent = await newOwnerConnection.WaitForRoleChangedEventAsync(
            e => e.GroupId == group.Id && e.NewRole == "Owner");

        Assert.Equal("Owner", roleEvent.NewRole);
    }
}