using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.DTOs.SignalR.PersonalChannel;
using AiGroupChat.IntegrationTests.Helpers;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Hubs.ChatHub;

/// <summary>
/// Integration tests for presence (online/offline) functionality
/// </summary>
[Collection("SignalR")]
public class PresenceTests : SignalRIntegrationTestBase
{
    public PresenceTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Connect_FirstConnection_SharedUsersReceiveOnline()
    {
        // Arrange - Two users in the same group
        AuthResponse user1 = await Auth.CreateAuthenticatedUserAsync(
            email: "online1@test.com",
            userName: "online1",
            displayName: "Online User 1");

        GroupResponse group = await Groups.CreateGroupAsync("Presence Group");

        Auth.ClearAuthToken();
        AuthResponse user2 = await Auth.CreateAuthenticatedUserAsync(
            email: "online2@test.com",
            userName: "online2");

        Auth.SetAuthToken(user1.AccessToken);
        await Members.AddMemberAsync(group.Id, user2.User.Id);

        // User2 connects first (will watch for User1's online event)
        SignalRHelper user2Connection = await CreateSignalRConnectionAsync(user2.AccessToken);

        // Act - User1 connects (should trigger online broadcast)
        SignalRHelper user1Connection = await CreateSignalRConnectionAsync(user1.AccessToken);

        // Assert - User2 should receive UserOnline for User1
        UserOnlineEvent onlineEvent = await user2Connection.WaitForUserOnlineEventAsync(
            e => e.UserId == user1.User.Id);

        Assert.Equal(user1.User.Id, onlineEvent.UserId);
        Assert.Equal("Online User 1", onlineEvent.DisplayName);
    }

    [Fact]
    public async Task Disconnect_LastConnection_SharedUsersReceiveOffline()
    {
        // Arrange - Two users in the same group
        AuthResponse user1 = await Auth.CreateAuthenticatedUserAsync(
            email: "offline1@test.com",
            userName: "offline1",
            displayName: "Offline User 1");

        GroupResponse group = await Groups.CreateGroupAsync("Offline Group");

        Auth.ClearAuthToken();
        AuthResponse user2 = await Auth.CreateAuthenticatedUserAsync(
            email: "offline2@test.com",
            userName: "offline2");

        Auth.SetAuthToken(user1.AccessToken);
        await Members.AddMemberAsync(group.Id, user2.User.Id);

        // Both users connect
        SignalRHelper user1Connection = await CreateSignalRConnectionAsync(user1.AccessToken);
        SignalRHelper user2Connection = await CreateSignalRConnectionAsync(user2.AccessToken);

        // Clear any online events from connection
        user2Connection.ClearEvents();

        // Act - User1 disconnects
        await user1Connection.DisconnectAsync();

        // Assert - User2 should receive UserOffline for User1
        UserOfflineEvent offlineEvent = await user2Connection.WaitForUserOfflineEventAsync(
            e => e.UserId == user1.User.Id);

        Assert.Equal(user1.User.Id, offlineEvent.UserId);
        Assert.Equal("Offline User 1", offlineEvent.DisplayName);
    }

    [Fact]
    public async Task Connect_NoSharedGroups_NoOnlineBroadcast()
    {
        // Arrange - Two users with no shared groups
        AuthResponse user1 = await Auth.CreateAuthenticatedUserAsync(
            email: "isolated1@test.com",
            userName: "isolated1");

        await Groups.CreateGroupAsync("User1 Group");

        Auth.ClearAuthToken();
        AuthResponse user2 = await Auth.CreateAuthenticatedUserAsync(
            email: "isolated2@test.com",
            userName: "isolated2");

        await Groups.CreateGroupAsync("User2 Group");

        // User2 connects first
        SignalRHelper user2Connection = await CreateSignalRConnectionAsync(user2.AccessToken);

        // Act - User1 connects
        SignalRHelper user1Connection = await CreateSignalRConnectionAsync(user1.AccessToken);

        // Assert - User2 should NOT receive any online event (no shared groups)
        await Task.Delay(500);
        Assert.Empty(user2Connection.UserOnlineEvents);
    }

    [Fact]
    public async Task OnlineEvent_ContainsCorrectUserInfo()
    {
        // Arrange
        AuthResponse user1 = await Auth.CreateAuthenticatedUserAsync(
            email: "infotest1@test.com",
            userName: "infotest1",
            displayName: "Info Test User");

        GroupResponse group = await Groups.CreateGroupAsync("Info Test Group");

        Auth.ClearAuthToken();
        AuthResponse user2 = await Auth.CreateAuthenticatedUserAsync(
            email: "infotest2@test.com",
            userName: "infotest2");

        Auth.SetAuthToken(user1.AccessToken);
        await Members.AddMemberAsync(group.Id, user2.User.Id);

        SignalRHelper user2Connection = await CreateSignalRConnectionAsync(user2.AccessToken);

        // Act
        SignalRHelper user1Connection = await CreateSignalRConnectionAsync(user1.AccessToken);

        // Assert
        UserOnlineEvent onlineEvent = await user2Connection.WaitForUserOnlineEventAsync(
            e => e.UserId == user1.User.Id);

        Assert.Equal(user1.User.Id, onlineEvent.UserId);
        Assert.Equal("Info Test User", onlineEvent.DisplayName);
        Assert.True(onlineEvent.OnlineAt <= DateTime.UtcNow);
        Assert.True(onlineEvent.OnlineAt > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task OfflineEvent_ContainsCorrectUserInfo()
    {
        // Arrange
        AuthResponse user1 = await Auth.CreateAuthenticatedUserAsync(
            email: "offlineinfo1@test.com",
            userName: "offlineinfo1",
            displayName: "Offline Info User");

        GroupResponse group = await Groups.CreateGroupAsync("Offline Info Group");

        Auth.ClearAuthToken();
        AuthResponse user2 = await Auth.CreateAuthenticatedUserAsync(
            email: "offlineinfo2@test.com",
            userName: "offlineinfo2");

        Auth.SetAuthToken(user1.AccessToken);
        await Members.AddMemberAsync(group.Id, user2.User.Id);

        SignalRHelper user1Connection = await CreateSignalRConnectionAsync(user1.AccessToken);
        SignalRHelper user2Connection = await CreateSignalRConnectionAsync(user2.AccessToken);
        user2Connection.ClearEvents();

        // Act
        await user1Connection.DisconnectAsync();

        // Assert
        UserOfflineEvent offlineEvent = await user2Connection.WaitForUserOfflineEventAsync(
            e => e.UserId == user1.User.Id);

        Assert.Equal(user1.User.Id, offlineEvent.UserId);
        Assert.Equal("Offline Info User", offlineEvent.DisplayName);
        Assert.True(offlineEvent.OfflineAt <= DateTime.UtcNow);
        Assert.True(offlineEvent.OfflineAt > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task MultipleSharedGroups_OnlyOneOnlineEvent()
    {
        // Arrange - Two users sharing multiple groups
        AuthResponse user1 = await Auth.CreateAuthenticatedUserAsync(
            email: "multi1@test.com",
            userName: "multi1");

        GroupResponse group1 = await Groups.CreateGroupAsync("Shared Group 1");
        GroupResponse group2 = await Groups.CreateGroupAsync("Shared Group 2");

        Auth.ClearAuthToken();
        AuthResponse user2 = await Auth.CreateAuthenticatedUserAsync(
            email: "multi2@test.com",
            userName: "multi2");

        Auth.SetAuthToken(user1.AccessToken);
        await Members.AddMemberAsync(group1.Id, user2.User.Id);
        await Members.AddMemberAsync(group2.Id, user2.User.Id);

        SignalRHelper user2Connection = await CreateSignalRConnectionAsync(user2.AccessToken);

        // Act
        SignalRHelper user1Connection = await CreateSignalRConnectionAsync(user1.AccessToken);

        // Assert - Should only receive ONE online event (not per-group)
        await user2Connection.WaitForUserOnlineEventAsync(e => e.UserId == user1.User.Id);

        // Wait a bit to ensure no duplicate events
        await Task.Delay(500);
        Assert.Single(user2Connection.UserOnlineEvents);
    }

    [Fact]
    public async Task Presence_OnlyToUsersInSharedGroups()
    {
        // Arrange - User1 and User2 share a group, User3 does not
        AuthResponse user1 = await Auth.CreateAuthenticatedUserAsync(
            email: "shared1@test.com",
            userName: "shared1");

        GroupResponse sharedGroup = await Groups.CreateGroupAsync("Shared Presence Group");

        Auth.ClearAuthToken();
        AuthResponse user2 = await Auth.CreateAuthenticatedUserAsync(
            email: "shared2@test.com",
            userName: "shared2");

        Auth.ClearAuthToken();
        AuthResponse user3 = await Auth.CreateAuthenticatedUserAsync(
            email: "notshared@test.com",
            userName: "notshared");

        // Only add user2 to the shared group, not user3
        Auth.SetAuthToken(user1.AccessToken);
        await Members.AddMemberAsync(sharedGroup.Id, user2.User.Id);

        // All users connect
        SignalRHelper user2Connection = await CreateSignalRConnectionAsync(user2.AccessToken);
        SignalRHelper user3Connection = await CreateSignalRConnectionAsync(user3.AccessToken);
        
        user2Connection.ClearEvents();
        user3Connection.ClearEvents();

        // Act - User1 connects
        SignalRHelper user1Connection = await CreateSignalRConnectionAsync(user1.AccessToken);

        // Assert
        // User2 should receive online event
        await user2Connection.WaitForUserOnlineEventAsync(e => e.UserId == user1.User.Id);

        // User3 should NOT receive online event
        await Task.Delay(500);
        Assert.Empty(user3Connection.UserOnlineEvents);
    }
}