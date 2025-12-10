using AiGroupChat.Application.DTOs.SignalR.PersonalChannel;
using Moq;

namespace AiGroupChat.UnitTests.Services.ChatHubService;

/// <summary>
/// Tests for ChatHubService presence methods (SendUserOnlineAsync, SendUserOfflineAsync)
/// </summary>
public class PresenceTests : ChatHubServiceTestBase
{
    #region SendUserOnlineAsync Tests

    [Fact]
    public async Task SendUserOnlineAsync_SendsToMultipleUsers()
    {
        // Arrange
        List<string> userIds = new List<string> { "user-1", "user-2", "user-3" };
        UserOnlineEvent eventData = CreateUserOnlineEvent();

        // Act
        await ChatHubService.SendUserOnlineAsync(userIds, eventData);

        // Assert
        Assert.NotNull(CapturedGroupNames);
        Assert.Equal(3, CapturedGroupNames.Count);
        Assert.Contains("user-user-1", CapturedGroupNames);
        Assert.Contains("user-user-2", CapturedGroupNames);
        Assert.Contains("user-user-3", CapturedGroupNames);
    }

    [Fact]
    public async Task SendUserOnlineAsync_SendsCorrectEventName()
    {
        // Arrange
        List<string> userIds = new List<string> { "user-1" };
        UserOnlineEvent eventData = CreateUserOnlineEvent();

        // Act
        await ChatHubService.SendUserOnlineAsync(userIds, eventData);

        // Assert
        Assert.Equal("UserOnline", CapturedMethodName);
    }

    [Fact]
    public async Task SendUserOnlineAsync_EmptyList_DoesNotSend()
    {
        // Arrange
        List<string> userIds = new List<string>();
        UserOnlineEvent eventData = CreateUserOnlineEvent();

        // Act
        await ChatHubService.SendUserOnlineAsync(userIds, eventData);

        // Assert - should not call SendCoreAsync
        GroupsClientProxyMock.Verify(
            c => c.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region SendUserOfflineAsync Tests

    [Fact]
    public async Task SendUserOfflineAsync_SendsToMultipleUsers()
    {
        // Arrange
        List<string> userIds = new List<string> { "user-1", "user-2" };
        UserOfflineEvent eventData = CreateUserOfflineEvent();

        // Act
        await ChatHubService.SendUserOfflineAsync(userIds, eventData);

        // Assert
        Assert.NotNull(CapturedGroupNames);
        Assert.Equal(2, CapturedGroupNames.Count);
        Assert.Contains("user-user-1", CapturedGroupNames);
        Assert.Contains("user-user-2", CapturedGroupNames);
    }

    [Fact]
    public async Task SendUserOfflineAsync_SendsCorrectEventName()
    {
        // Arrange
        List<string> userIds = new List<string> { "user-1" };
        UserOfflineEvent eventData = CreateUserOfflineEvent();

        // Act
        await ChatHubService.SendUserOfflineAsync(userIds, eventData);

        // Assert
        Assert.Equal("UserOffline", CapturedMethodName);
    }

    [Fact]
    public async Task SendUserOfflineAsync_EmptyList_DoesNotSend()
    {
        // Arrange
        List<string> userIds = new List<string>();
        UserOfflineEvent eventData = CreateUserOfflineEvent();

        // Act
        await ChatHubService.SendUserOfflineAsync(userIds, eventData);

        // Assert - should not call SendCoreAsync
        GroupsClientProxyMock.Verify(
            c => c.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Helper Methods

    private UserOnlineEvent CreateUserOnlineEvent()
    {
        return new UserOnlineEvent
        {
            UserId = TestUserId,
            DisplayName = "Test User",
            OnlineAt = DateTime.UtcNow
        };
    }

    private UserOfflineEvent CreateUserOfflineEvent()
    {
        return new UserOfflineEvent
        {
            UserId = TestUserId,
            DisplayName = "Test User",
            OfflineAt = DateTime.UtcNow
        };
    }

    #endregion
}