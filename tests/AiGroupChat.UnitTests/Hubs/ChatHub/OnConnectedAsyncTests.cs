using AiGroupChat.Application.DTOs.SignalR.PersonalChannel;
using Moq;

namespace AiGroupChat.UnitTests.Hubs.ChatHub;

/// <summary>
/// Tests for the ChatHub.OnConnectedAsync method
/// </summary>
public class OnConnectedAsyncTests : ChatHubTestBase
{
    [Fact]
    public async Task OnConnectedAsync_AddsToPersonalChannel()
    {
        // Arrange
        SetupConnectionTracker(isFirstConnection: false);
        SetupSharedUsers(new List<string>());

        // Act
        await Hub.OnConnectedAsync();

        // Assert - should join personal channel
        GroupsMock.Verify(
            g => g.AddToGroupAsync(
                TestConnectionId,
                GetPersonalChannelName(TestUserId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task OnConnectedAsync_TracksConnection()
    {
        // Arrange
        SetupConnectionTracker(isFirstConnection: false);
        SetupSharedUsers(new List<string>());

        // Act
        await Hub.OnConnectedAsync();

        // Assert
        ConnectionTrackerMock.Verify(
            t => t.AddConnection(TestUserId, TestConnectionId),
            Times.Once);
    }

    [Fact]
    public async Task OnConnectedAsync_FirstConnection_BroadcastsUserOnline()
    {
        // Arrange
        SetupConnectionTracker(isFirstConnection: true);
        SetupUserExists();
        List<string> sharedUsers = new List<string> { "user-2", "user-3" };
        SetupSharedUsers(sharedUsers);

        // Act
        await Hub.OnConnectedAsync();

        // Assert
        ChatHubServiceMock.Verify(
            s => s.SendUserOnlineAsync(
                sharedUsers,
                It.Is<UserOnlineEvent>(e => e.UserId == TestUserId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task OnConnectedAsync_SecondConnection_DoesNotBroadcastOnline()
    {
        // Arrange
        SetupConnectionTracker(isFirstConnection: false);
        SetupSharedUsers(new List<string> { "user-2" });

        // Act
        await Hub.OnConnectedAsync();

        // Assert - should NOT broadcast online
        ChatHubServiceMock.Verify(
            s => s.SendUserOnlineAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<UserOnlineEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnConnectedAsync_FirstConnection_NoSharedUsers_DoesNotBroadcast()
    {
        // Arrange
        SetupConnectionTracker(isFirstConnection: true);
        SetupUserExists();
        SetupSharedUsers(new List<string>()); // Empty list

        // Act
        await Hub.OnConnectedAsync();

        // Assert - should NOT broadcast (no one to broadcast to)
        ChatHubServiceMock.Verify(
            s => s.SendUserOnlineAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<UserOnlineEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}