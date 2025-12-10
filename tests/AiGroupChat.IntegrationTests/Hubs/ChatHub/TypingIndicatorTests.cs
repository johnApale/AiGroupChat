using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.DTOs.SignalR.GroupChannel;
using AiGroupChat.IntegrationTests.Helpers;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Hubs.ChatHub;

/// <summary>
/// Integration tests for typing indicator functionality
/// </summary>
[Collection("SignalR")]
public class TypingIndicatorTests : SignalRIntegrationTestBase
{
    public TypingIndicatorTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task StartTyping_OtherMembersReceiveEvent()
    {
        // Arrange - Two users in same group
        AuthResponse user1 = await Auth.CreateAuthenticatedUserAsync(
            email: "typer@test.com",
            userName: "typer");

        GroupResponse group = await Groups.CreateGroupAsync("Typing Group");

        // Add second user
        Auth.ClearAuthToken();
        AuthResponse user2 = await Auth.CreateAuthenticatedUserAsync(
            email: "watcher@test.com",
            userName: "watcher");

        Auth.SetAuthToken(user1.AccessToken);
        await Members.AddMemberAsync(group.Id, user2.User.Id);

        // Both users connect and join the group
        SignalRHelper user1Connection = await CreateSignalRConnectionAsync(user1.AccessToken);
        SignalRHelper user2Connection = await CreateSignalRConnectionAsync(user2.AccessToken);

        await user1Connection.JoinGroupAsync(group.Id);
        await user2Connection.JoinGroupAsync(group.Id);

        // Act - User1 starts typing
        await user1Connection.StartTypingAsync(group.Id);

        // Assert - User2 should receive typing event
        UserTypingEvent typingEvent = await user2Connection.WaitForTypingEventAsync(
            e => e.UserId == user1.User.Id);

        Assert.Equal(group.Id, typingEvent.GroupId);
        Assert.Equal(user1.User.Id, typingEvent.UserId);
    }

    [Fact]
    public async Task StartTyping_SenderDoesNotReceiveOwnEvent()
    {
        // Arrange
        AuthResponse user = await Auth.CreateAuthenticatedUserAsync(
            email: "selftyper@test.com",
            userName: "selftyper");

        GroupResponse group = await Groups.CreateGroupAsync("Self Type Group");

        SignalRHelper connection = await CreateSignalRConnectionAsync(user.AccessToken);
        await connection.JoinGroupAsync(group.Id);

        // Act
        await connection.StartTypingAsync(group.Id);

        // Assert - Should NOT receive own typing event
        await Task.Delay(500);
        Assert.Empty(connection.TypingEvents);
    }

    [Fact]
    public async Task StopTyping_OtherMembersReceiveEvent()
    {
        // Arrange - Two users in same group
        AuthResponse user1 = await Auth.CreateAuthenticatedUserAsync(
            email: "stopper@test.com",
            userName: "stopper");

        GroupResponse group = await Groups.CreateGroupAsync("Stop Typing Group");

        Auth.ClearAuthToken();
        AuthResponse user2 = await Auth.CreateAuthenticatedUserAsync(
            email: "observer@test.com",
            userName: "observer");

        Auth.SetAuthToken(user1.AccessToken);
        await Members.AddMemberAsync(group.Id, user2.User.Id);

        SignalRHelper user1Connection = await CreateSignalRConnectionAsync(user1.AccessToken);
        SignalRHelper user2Connection = await CreateSignalRConnectionAsync(user2.AccessToken);

        await user1Connection.JoinGroupAsync(group.Id);
        await user2Connection.JoinGroupAsync(group.Id);

        // Act - User1 stops typing
        await user1Connection.StopTypingAsync(group.Id);

        // Assert - User2 should receive stopped typing event
        UserStoppedTypingEvent stoppedEvent = await user2Connection.WaitForStoppedTypingEventAsync(
            e => e.UserId == user1.User.Id);

        Assert.Equal(group.Id, stoppedEvent.GroupId);
        Assert.Equal(user1.User.Id, stoppedEvent.UserId);
    }

    [Fact]
    public async Task StartTyping_NonMember_NoEventBroadcast()
    {
        // Arrange - User1 owns group, User2 is not a member
        AuthResponse user1 = await Auth.CreateAuthenticatedUserAsync(
            email: "groupowner@test.com",
            userName: "groupowner");

        GroupResponse group = await Groups.CreateGroupAsync("Private Group");

        Auth.ClearAuthToken();
        AuthResponse user2 = await Auth.CreateAuthenticatedUserAsync(
            email: "outsider@test.com",
            userName: "outsider");

        // User1 joins SignalR group
        SignalRHelper user1Connection = await CreateSignalRConnectionAsync(user1.AccessToken);
        await user1Connection.JoinGroupAsync(group.Id);

        // User2 connects but can't join the group (not a member)
        SignalRHelper user2Connection = await CreateSignalRConnectionAsync(user2.AccessToken);

        // Act - User2 tries to start typing (should be silently ignored)
        await user2Connection.StartTypingAsync(group.Id);

        // Assert - User1 should NOT receive any typing event
        await Task.Delay(500);
        Assert.Empty(user1Connection.TypingEvents);
    }

    [Fact]
    public async Task TypingEvent_ContainsCorrectUserInfo()
    {
        // Arrange
        AuthResponse user1 = await Auth.CreateAuthenticatedUserAsync(
            email: "infouser@test.com",
            userName: "infouser",
            displayName: "Info User Display");

        GroupResponse group = await Groups.CreateGroupAsync("Info Group");

        Auth.ClearAuthToken();
        AuthResponse user2 = await Auth.CreateAuthenticatedUserAsync(
            email: "listener@test.com",
            userName: "listener");

        Auth.SetAuthToken(user1.AccessToken);
        await Members.AddMemberAsync(group.Id, user2.User.Id);

        SignalRHelper user1Connection = await CreateSignalRConnectionAsync(user1.AccessToken);
        SignalRHelper user2Connection = await CreateSignalRConnectionAsync(user2.AccessToken);

        await user1Connection.JoinGroupAsync(group.Id);
        await user2Connection.JoinGroupAsync(group.Id);

        // Act
        await user1Connection.StartTypingAsync(group.Id);

        // Assert
        UserTypingEvent typingEvent = await user2Connection.WaitForTypingEventAsync(
            e => e.UserId == user1.User.Id);

        Assert.Equal("infouser", typingEvent.UserName);
        Assert.Equal("Info User Display", typingEvent.DisplayName);
    }
}