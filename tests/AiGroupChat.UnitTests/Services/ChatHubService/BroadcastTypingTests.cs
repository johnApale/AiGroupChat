using AiGroupChat.Application.DTOs.SignalR.GroupChannel;

namespace AiGroupChat.UnitTests.Services.ChatHubService;

/// <summary>
/// Tests for ChatHubService typing broadcasts (UserTyping, UserStoppedTyping)
/// </summary>
public class BroadcastTypingTests : ChatHubServiceTestBase
{
    #region UserTyping Tests

    [Fact]
    public async Task BroadcastUserTypingAsync_SendsToCorrectGroup()
    {
        // Arrange
        UserTypingEvent eventData = CreateUserTypingEvent();

        // Act
        await ChatHubService.BroadcastUserTypingAsync(TestGroupId, eventData);

        // Assert
        Assert.Equal(GetGroupName(TestGroupId), CapturedGroupName);
    }

    [Fact]
    public async Task BroadcastUserTypingAsync_SendsCorrectEventName()
    {
        // Arrange
        UserTypingEvent eventData = CreateUserTypingEvent();

        // Act
        await ChatHubService.BroadcastUserTypingAsync(TestGroupId, eventData);

        // Assert
        Assert.Equal("UserTyping", CapturedMethodName);
    }

    [Fact]
    public async Task BroadcastUserTypingAsync_SendsCorrectPayload()
    {
        // Arrange
        UserTypingEvent eventData = CreateUserTypingEvent();

        // Act
        await ChatHubService.BroadcastUserTypingAsync(TestGroupId, eventData);

        // Assert
        Assert.NotNull(CapturedArgs);
        Assert.Single(CapturedArgs);
        Assert.Same(eventData, CapturedArgs[0]);
    }

    #endregion

    #region UserStoppedTyping Tests

    [Fact]
    public async Task BroadcastUserStoppedTypingAsync_SendsToCorrectGroup()
    {
        // Arrange
        UserStoppedTypingEvent eventData = CreateUserStoppedTypingEvent();

        // Act
        await ChatHubService.BroadcastUserStoppedTypingAsync(TestGroupId, eventData);

        // Assert
        Assert.Equal(GetGroupName(TestGroupId), CapturedGroupName);
    }

    [Fact]
    public async Task BroadcastUserStoppedTypingAsync_SendsCorrectEventName()
    {
        // Arrange
        UserStoppedTypingEvent eventData = CreateUserStoppedTypingEvent();

        // Act
        await ChatHubService.BroadcastUserStoppedTypingAsync(TestGroupId, eventData);

        // Assert
        Assert.Equal("UserStoppedTyping", CapturedMethodName);
    }

    [Fact]
    public async Task BroadcastUserStoppedTypingAsync_SendsCorrectPayload()
    {
        // Arrange
        UserStoppedTypingEvent eventData = CreateUserStoppedTypingEvent();

        // Act
        await ChatHubService.BroadcastUserStoppedTypingAsync(TestGroupId, eventData);

        // Assert
        Assert.NotNull(CapturedArgs);
        Assert.Single(CapturedArgs);
        Assert.Same(eventData, CapturedArgs[0]);
    }

    #endregion

    #region Helper Methods

    private UserTypingEvent CreateUserTypingEvent()
    {
        return new UserTypingEvent
        {
            GroupId = TestGroupId,
            UserId = TestUserId,
            UserName = "testuser",
            DisplayName = "Test User"
        };
    }

    private UserStoppedTypingEvent CreateUserStoppedTypingEvent()
    {
        return new UserStoppedTypingEvent
        {
            GroupId = TestGroupId,
            UserId = TestUserId
        };
    }

    #endregion
}