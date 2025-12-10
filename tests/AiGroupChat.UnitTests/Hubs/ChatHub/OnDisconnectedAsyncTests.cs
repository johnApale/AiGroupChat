using AiGroupChat.Application.DTOs.SignalR.PersonalChannel;
using Moq;

namespace AiGroupChat.UnitTests.Hubs.ChatHub;

/// <summary>
/// Tests for the ChatHub.OnDisconnectedAsync method
/// </summary>
public class OnDisconnectedAsyncTests : ChatHubTestBase
{
    [Fact]
    public async Task OnDisconnectedAsync_RemovesConnection()
    {
        // Arrange
        SetupConnectionTracker(isLastConnection: false);
        SetupSharedUsers(new List<string>());

        // Act
        await Hub.OnDisconnectedAsync(exception: null);

        // Assert
        ConnectionTrackerMock.Verify(
            t => t.RemoveConnection(TestUserId, TestConnectionId),
            Times.Once);
    }

    [Fact]
    public async Task OnDisconnectedAsync_LastConnection_BroadcastsUserOffline()
    {
        // Arrange
        SetupConnectionTracker(isLastConnection: true);
        SetupUserExists();
        List<string> sharedUsers = new List<string> { "user-2", "user-3" };
        SetupSharedUsers(sharedUsers);

        // Act
        await Hub.OnDisconnectedAsync(exception: null);

        // Assert
        ChatHubServiceMock.Verify(
            s => s.SendUserOfflineAsync(
                sharedUsers,
                It.Is<UserOfflineEvent>(e => e.UserId == TestUserId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task OnDisconnectedAsync_NotLastConnection_DoesNotBroadcast()
    {
        // Arrange
        SetupConnectionTracker(isLastConnection: false);
        SetupSharedUsers(new List<string> { "user-2" });

        // Act
        await Hub.OnDisconnectedAsync(exception: null);

        // Assert - should NOT broadcast offline
        ChatHubServiceMock.Verify(
            s => s.SendUserOfflineAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<UserOfflineEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnDisconnectedAsync_LastConnection_NoSharedUsers_DoesNotBroadcast()
    {
        // Arrange
        SetupConnectionTracker(isLastConnection: true);
        SetupUserExists();
        SetupSharedUsers(new List<string>()); // Empty list

        // Act
        await Hub.OnDisconnectedAsync(exception: null);

        // Assert - should NOT broadcast (no one to broadcast to)
        ChatHubServiceMock.Verify(
            s => s.SendUserOfflineAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<UserOfflineEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnDisconnectedAsync_WithException_StillRemovesConnection()
    {
        // Arrange
        SetupConnectionTracker(isLastConnection: false);
        SetupSharedUsers(new List<string>());
        Exception testException = new Exception("Connection lost");

        // Act
        await Hub.OnDisconnectedAsync(exception: testException);

        // Assert - should still remove connection even with exception
        ConnectionTrackerMock.Verify(
            t => t.RemoveConnection(TestUserId, TestConnectionId),
            Times.Once);
    }
}