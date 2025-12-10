using AiGroupChat.Application.DTOs.SignalR.PersonalChannel;

namespace AiGroupChat.UnitTests.Services.ChatHubService;

/// <summary>
/// Tests for ChatHubService personal channel methods
/// </summary>
public class PersonalChannelTests : ChatHubServiceTestBase
{
    #region SendGroupActivityAsync Tests

    [Fact]
    public async Task SendGroupActivityAsync_SendsToPersonalChannel()
    {
        // Arrange
        GroupActivityEvent eventData = CreateGroupActivityEvent();

        // Act
        await ChatHubService.SendGroupActivityAsync(TestUserId, eventData);

        // Assert
        Assert.Equal(GetPersonalChannelName(TestUserId), CapturedGroupName);
    }

    [Fact]
    public async Task SendGroupActivityAsync_SendsCorrectEventName()
    {
        // Arrange
        GroupActivityEvent eventData = CreateGroupActivityEvent();

        // Act
        await ChatHubService.SendGroupActivityAsync(TestUserId, eventData);

        // Assert
        Assert.Equal("GroupActivity", CapturedMethodName);
    }

    #endregion

    #region SendNewMessageNotificationAsync Tests

    [Fact]
    public async Task SendNewMessageNotificationAsync_SendsToPersonalChannel()
    {
        // Arrange
        NewMessageNotificationEvent eventData = CreateNewMessageNotificationEvent();

        // Act
        await ChatHubService.SendNewMessageNotificationAsync(TestUserId, eventData);

        // Assert
        Assert.Equal(GetPersonalChannelName(TestUserId), CapturedGroupName);
    }

    [Fact]
    public async Task SendNewMessageNotificationAsync_SendsCorrectEventName()
    {
        // Arrange
        NewMessageNotificationEvent eventData = CreateNewMessageNotificationEvent();

        // Act
        await ChatHubService.SendNewMessageNotificationAsync(TestUserId, eventData);

        // Assert
        Assert.Equal("NewMessageNotification", CapturedMethodName);
    }

    #endregion

    #region SendAddedToGroupAsync Tests

    [Fact]
    public async Task SendAddedToGroupAsync_SendsToPersonalChannel()
    {
        // Arrange
        AddedToGroupEvent eventData = CreateAddedToGroupEvent();

        // Act
        await ChatHubService.SendAddedToGroupAsync(TestUserId, eventData);

        // Assert
        Assert.Equal(GetPersonalChannelName(TestUserId), CapturedGroupName);
    }

    [Fact]
    public async Task SendAddedToGroupAsync_SendsCorrectEventName()
    {
        // Arrange
        AddedToGroupEvent eventData = CreateAddedToGroupEvent();

        // Act
        await ChatHubService.SendAddedToGroupAsync(TestUserId, eventData);

        // Assert
        Assert.Equal("AddedToGroup", CapturedMethodName);
    }

    #endregion

    #region SendRemovedFromGroupAsync Tests

    [Fact]
    public async Task SendRemovedFromGroupAsync_SendsToPersonalChannel()
    {
        // Arrange
        RemovedFromGroupEvent eventData = CreateRemovedFromGroupEvent();

        // Act
        await ChatHubService.SendRemovedFromGroupAsync(TestUserId, eventData);

        // Assert
        Assert.Equal(GetPersonalChannelName(TestUserId), CapturedGroupName);
    }

    [Fact]
    public async Task SendRemovedFromGroupAsync_SendsCorrectEventName()
    {
        // Arrange
        RemovedFromGroupEvent eventData = CreateRemovedFromGroupEvent();

        // Act
        await ChatHubService.SendRemovedFromGroupAsync(TestUserId, eventData);

        // Assert
        Assert.Equal("RemovedFromGroup", CapturedMethodName);
    }

    #endregion

    #region SendRoleChangedAsync Tests

    [Fact]
    public async Task SendRoleChangedAsync_SendsToPersonalChannel()
    {
        // Arrange
        RoleChangedEvent eventData = CreateRoleChangedEvent();

        // Act
        await ChatHubService.SendRoleChangedAsync(TestUserId, eventData);

        // Assert
        Assert.Equal(GetPersonalChannelName(TestUserId), CapturedGroupName);
    }

    [Fact]
    public async Task SendRoleChangedAsync_SendsCorrectEventName()
    {
        // Arrange
        RoleChangedEvent eventData = CreateRoleChangedEvent();

        // Act
        await ChatHubService.SendRoleChangedAsync(TestUserId, eventData);

        // Assert
        Assert.Equal("RoleChanged", CapturedMethodName);
    }

    #endregion

    #region Helper Methods

    private GroupActivityEvent CreateGroupActivityEvent()
    {
        return new GroupActivityEvent
        {
            GroupId = TestGroupId,
            GroupName = "Test Group",
            ActivityType = "NewMessage",
            Timestamp = DateTime.UtcNow,
            Preview = "Hello...",
            ActorName = "Test User"
        };
    }

    private NewMessageNotificationEvent CreateNewMessageNotificationEvent()
    {
        return new NewMessageNotificationEvent
        {
            GroupId = TestGroupId,
            GroupName = "Test Group",
            MessageId = Guid.NewGuid(),
            SenderName = "Test User",
            Preview = "Hello...",
            SentAt = DateTime.UtcNow
        };
    }

    private AddedToGroupEvent CreateAddedToGroupEvent()
    {
        return new AddedToGroupEvent
        {
            GroupId = TestGroupId,
            GroupName = "Test Group"
        };
    }

    private RemovedFromGroupEvent CreateRemovedFromGroupEvent()
    {
        return new RemovedFromGroupEvent
        {
            GroupId = TestGroupId,
            GroupName = "Test Group"
        };
    }

    private RoleChangedEvent CreateRoleChangedEvent()
    {
        return new RoleChangedEvent
        {
            GroupId = TestGroupId,
            GroupName = "Test Group",
            NewRole = "Admin"
        };
    }

    #endregion
}