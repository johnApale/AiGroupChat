using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.IntegrationTests.Helpers;
using AiGroupChat.IntegrationTests.Infrastructure;
using MessageResponse = AiGroupChat.Application.DTOs.Messages.MessageResponse;

namespace AiGroupChat.IntegrationTests.Hubs.ChatHub;

/// <summary>
/// Integration tests for message broadcasting via SignalR
/// </summary>
[Collection("SignalR")]
public class MessageBroadcastTests : SignalRIntegrationTestBase
{
    public MessageBroadcastTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task SendMessage_JoinedMembersReceiveViaWebSocket()
    {
        // Arrange
        AuthResponse user = await Auth.CreateAuthenticatedUserAsync(
            email: "sender@test.com",
            userName: "sender");

        GroupResponse group = await Groups.CreateGroupAsync("Broadcast Group");

        SignalRHelper connection = await CreateSignalRConnectionAsync(user.AccessToken);
        await connection.JoinGroupAsync(group.Id);

        // Act - Send message via REST API
        await Messages.SendMessageAsync(group.Id, "Test broadcast message");

        // Assert - Should receive via WebSocket
        MessageResponse received = await connection.WaitForMessageAsync(
            m => m.Content == "Test broadcast message");

        Assert.NotNull(received);
        Assert.Equal(group.Id, received.GroupId);
    }

    [Fact]
    public async Task SendMessage_MessageContainsAllFields()
    {
        // Arrange
        AuthResponse user = await Auth.CreateAuthenticatedUserAsync(
            email: "fieldsuser@test.com",
            userName: "fieldsuser",
            displayName: "Fields User");

        GroupResponse group = await Groups.CreateGroupAsync("Fields Group");

        SignalRHelper connection = await CreateSignalRConnectionAsync(user.AccessToken);
        await connection.JoinGroupAsync(group.Id);

        // Act
        await Messages.SendMessageAsync(group.Id, "Full message content");

        // Assert
        MessageResponse received = await connection.WaitForMessageAsync(
            m => m.Content == "Full message content");

        Assert.NotEqual(Guid.Empty, received.Id);
        Assert.Equal(group.Id, received.GroupId);
        Assert.Equal(user.User.Id, received.SenderId);
        Assert.Equal("fieldsuser", received.SenderUserName);
        Assert.Equal("Fields User", received.SenderDisplayName);
        Assert.Equal("User", received.SenderType);
        Assert.Equal("Full message content", received.Content);
        Assert.True(received.CreatedAt > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task SendMessage_MultipleJoinedUsers_AllReceive()
    {
        // Arrange - Three users in the same group
        AuthResponse user1 = await Auth.CreateAuthenticatedUserAsync(
            email: "multi1@test.com",
            userName: "multi1");

        GroupResponse group = await Groups.CreateGroupAsync("Multi User Group");

        Auth.ClearAuthToken();
        AuthResponse user2 = await Auth.CreateAuthenticatedUserAsync(
            email: "multi2@test.com",
            userName: "multi2");

        Auth.ClearAuthToken();
        AuthResponse user3 = await Auth.CreateAuthenticatedUserAsync(
            email: "multi3@test.com",
            userName: "multi3");

        // Add users to group
        Auth.SetAuthToken(user1.AccessToken);
        await Members.AddMemberAsync(group.Id, user2.User.Id);
        await Members.AddMemberAsync(group.Id, user3.User.Id);

        // All users connect and join
        SignalRHelper conn1 = await CreateSignalRConnectionAsync(user1.AccessToken);
        SignalRHelper conn2 = await CreateSignalRConnectionAsync(user2.AccessToken);
        SignalRHelper conn3 = await CreateSignalRConnectionAsync(user3.AccessToken);

        await conn1.JoinGroupAsync(group.Id);
        await conn2.JoinGroupAsync(group.Id);
        await conn3.JoinGroupAsync(group.Id);

        // Act - User1 sends a message
        await Messages.SendMessageAsync(group.Id, "Message to everyone");

        // Assert - All three should receive
        await conn1.WaitForMessageAsync(m => m.Content == "Message to everyone");
        await conn2.WaitForMessageAsync(m => m.Content == "Message to everyone");
        await conn3.WaitForMessageAsync(m => m.Content == "Message to everyone");

        Assert.Single(conn1.ReceivedMessages);
        Assert.Single(conn2.ReceivedMessages);
        Assert.Single(conn3.ReceivedMessages);
    }

    [Fact]
    public async Task SendMessage_SenderAlsoReceives()
    {
        // Arrange
        AuthResponse user = await Auth.CreateAuthenticatedUserAsync(
            email: "selfsend@test.com",
            userName: "selfsend");

        GroupResponse group = await Groups.CreateGroupAsync("Self Send Group");

        SignalRHelper connection = await CreateSignalRConnectionAsync(user.AccessToken);
        await connection.JoinGroupAsync(group.Id);

        // Act - User sends a message
        await Messages.SendMessageAsync(group.Id, "My own message");

        // Assert - Sender should also receive (for UI consistency)
        MessageResponse received = await connection.WaitForMessageAsync(
            m => m.Content == "My own message");

        Assert.Equal(user.User.Id, received.SenderId);
    }

    [Fact]
    public async Task SendMessage_NonGroupMembers_DoNotReceive()
    {
        // Arrange - Two separate groups, user in one group shouldn't see messages from other
        AuthResponse user1 = await Auth.CreateAuthenticatedUserAsync(
            email: "isolated1@test.com",
            userName: "isolated1");

        GroupResponse group1 = await Groups.CreateGroupAsync("Isolated Group 1");

        Auth.ClearAuthToken();
        AuthResponse user2 = await Auth.CreateAuthenticatedUserAsync(
            email: "isolated2@test.com",
            userName: "isolated2");

        GroupResponse group2 = await Groups.CreateGroupAsync("Isolated Group 2");

        // Each user joins their own group only
        SignalRHelper conn1 = await CreateSignalRConnectionAsync(user1.AccessToken);
        SignalRHelper conn2 = await CreateSignalRConnectionAsync(user2.AccessToken);

        await conn1.JoinGroupAsync(group1.Id);
        await conn2.JoinGroupAsync(group2.Id);

        // Act - User1 sends to group1
        Auth.SetAuthToken(user1.AccessToken);
        await Messages.SendMessageAsync(group1.Id, "Secret message");

        // Assert - User1 receives, User2 does not
        await conn1.WaitForMessageAsync(m => m.Content == "Secret message");

        await Task.Delay(500);
        Assert.Empty(conn2.ReceivedMessages);
    }

    [Fact]
    public async Task SendMessage_NotJoinedSignalR_DoesNotReceive()
    {
        // Arrange - User is a member of group but didn't join SignalR group
        AuthResponse user = await Auth.CreateAuthenticatedUserAsync(
            email: "notjoined@test.com",
            userName: "notjoined");

        GroupResponse group = await Groups.CreateGroupAsync("Not Joined Group");

        SignalRHelper connection = await CreateSignalRConnectionAsync(user.AccessToken);
        // Intentionally NOT joining: await connection.JoinGroupAsync(group.Id);

        // Act
        await Messages.SendMessageAsync(group.Id, "Won't be received");

        // Assert
        await Task.Delay(500);
        Assert.Empty(connection.ReceivedMessages);
    }

    [Fact]
    public async Task SendMessage_RapidMessages_AllReceived()
    {
        // Arrange
        AuthResponse user = await Auth.CreateAuthenticatedUserAsync(
            email: "rapid@test.com",
            userName: "rapid");

        GroupResponse group = await Groups.CreateGroupAsync("Rapid Group");

        SignalRHelper connection = await CreateSignalRConnectionAsync(user.AccessToken);
        await connection.JoinGroupAsync(group.Id);

        // Act - Send multiple messages quickly
        await Messages.SendMessageAsync(group.Id, "Message 1");
        await Messages.SendMessageAsync(group.Id, "Message 2");
        await Messages.SendMessageAsync(group.Id, "Message 3");

        // Assert - All should be received
        await connection.WaitForMessageAsync(m => m.Content == "Message 1");
        await connection.WaitForMessageAsync(m => m.Content == "Message 2");
        await connection.WaitForMessageAsync(m => m.Content == "Message 3");

        Assert.Equal(3, connection.ReceivedMessages.Count);
    }
}