using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.IntegrationTests.Helpers;
using AiGroupChat.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace AiGroupChat.IntegrationTests.Hubs.ChatHub;

/// <summary>
/// Integration tests for joining and leaving SignalR groups
/// </summary>
[Collection("SignalR")]
public class JoinLeaveGroupTests : SignalRIntegrationTestBase
{
    public JoinLeaveGroupTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task JoinGroup_WhenMember_Succeeds()
    {
        // Arrange
        AuthResponse user = await Auth.CreateAuthenticatedUserAsync(
            email: "join@test.com",
            userName: "joinuser");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");
        SignalRHelper connection = await CreateSignalRConnectionAsync(user.AccessToken);

        // Act & Assert - should not throw
        await connection.JoinGroupAsync(group.Id);
    }

    [Fact]
    public async Task JoinGroup_WhenNotMember_ThrowsHubException()
    {
        // Arrange - Create two users, one creates group, other tries to join
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@test.com",
            userName: "owner");

        GroupResponse group = await Groups.CreateGroupAsync("Owner's Group");

        // Create second user (not a member of the group)
        Auth.ClearAuthToken();
        AuthResponse nonMember = await Auth.CreateAuthenticatedUserAsync(
            email: "nonmember@test.com",
            userName: "nonmember");

        SignalRHelper connection = await CreateSignalRConnectionAsync(nonMember.AccessToken);

        // Act & Assert
        HubException exception = await Assert.ThrowsAsync<HubException>(
            () => connection.JoinGroupAsync(group.Id));

        Assert.Contains("not a member", exception.Message);
    }

    [Fact]
    public async Task JoinGroup_ReceivesSubsequentMessages()
    {
        // Arrange
        AuthResponse user = await Auth.CreateAuthenticatedUserAsync(
            email: "receiver@test.com",
            userName: "receiver");

        GroupResponse group = await Groups.CreateGroupAsync("Message Group");
        SignalRHelper connection = await CreateSignalRConnectionAsync(user.AccessToken);

        await connection.JoinGroupAsync(group.Id);

        // Act - Send a message via REST API
        await Messages.SendMessageAsync(group.Id, "Hello from REST!");

        // Assert - Should receive the message via WebSocket
        Application.DTOs.Messages.MessageResponse receivedMessage = 
            await connection.WaitForMessageAsync(m => m.Content == "Hello from REST!");

        Assert.Equal(group.Id, receivedMessage.GroupId);
        Assert.Equal("Hello from REST!", receivedMessage.Content);
    }

    [Fact]
    public async Task LeaveGroup_StopsReceivingMessages()
    {
        // Arrange
        AuthResponse user = await Auth.CreateAuthenticatedUserAsync(
            email: "leaver@test.com",
            userName: "leaver");

        GroupResponse group = await Groups.CreateGroupAsync("Leave Group");
        SignalRHelper connection = await CreateSignalRConnectionAsync(user.AccessToken);

        await connection.JoinGroupAsync(group.Id);
        await connection.LeaveGroupAsync(group.Id);
        connection.ClearEvents();

        // Act - Send a message after leaving
        await Messages.SendMessageAsync(group.Id, "Message after leave");

        // Assert - Should NOT receive the message (wait briefly then check)
        await Task.Delay(500);
        Assert.Empty(connection.ReceivedMessages);
    }

    [Fact]
    public async Task JoinGroup_MultipleGroups_ReceivesFromAll()
    {
        // Arrange
        AuthResponse user = await Auth.CreateAuthenticatedUserAsync(
            email: "multi@test.com",
            userName: "multiuser");

        GroupResponse group1 = await Groups.CreateGroupAsync("Group 1");
        GroupResponse group2 = await Groups.CreateGroupAsync("Group 2");

        SignalRHelper connection = await CreateSignalRConnectionAsync(user.AccessToken);

        await connection.JoinGroupAsync(group1.Id);
        await connection.JoinGroupAsync(group2.Id);

        // Act - Send messages to both groups
        await Messages.SendMessageAsync(group1.Id, "Message to Group 1");
        await Messages.SendMessageAsync(group2.Id, "Message to Group 2");

        // Assert - Should receive both messages
        await connection.WaitForMessageAsync(m => m.Content == "Message to Group 1");
        await connection.WaitForMessageAsync(m => m.Content == "Message to Group 2");

        Assert.Equal(2, connection.ReceivedMessages.Count);
    }

    [Fact]
    public async Task JoinGroup_OnlyJoinedUserReceivesMessages()
    {
        // Arrange - Two users in same group, only one joins SignalR
        AuthResponse user1 = await Auth.CreateAuthenticatedUserAsync(
            email: "user1@test.com",
            userName: "user1");

        GroupResponse group = await Groups.CreateGroupAsync("Shared Group");

        // Add second user to the group
        Auth.ClearAuthToken();
        AuthResponse user2 = await Auth.CreateAuthenticatedUserAsync(
            email: "user2@test.com",
            userName: "user2");

        // Switch back to user1 to add user2 to the group
        Auth.SetAuthToken(user1.AccessToken);
        await Members.AddMemberAsync(group.Id, user2.User.Id);

        // User1 joins SignalR group
        SignalRHelper user1Connection = await CreateSignalRConnectionAsync(user1.AccessToken);
        await user1Connection.JoinGroupAsync(group.Id);

        // User2 connects but does NOT join the SignalR group
        SignalRHelper user2Connection = await CreateSignalRConnectionAsync(user2.AccessToken);
        // Intentionally not calling: await user2Connection.JoinGroupAsync(group.Id);

        // Act - User2 sends a message via REST
        Auth.SetAuthToken(user2.AccessToken);
        await Messages.SendMessageAsync(group.Id, "Hello from User2!");

        // Assert - User1 should receive it, User2 should not (not joined SignalR group)
        await user1Connection.WaitForMessageAsync(m => m.Content == "Hello from User2!");
        Assert.Single(user1Connection.ReceivedMessages);

        await Task.Delay(500);
        Assert.Empty(user2Connection.ReceivedMessages);
    }
}