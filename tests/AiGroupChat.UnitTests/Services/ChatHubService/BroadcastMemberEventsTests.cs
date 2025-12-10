using AiGroupChat.Application.DTOs.SignalR.GroupChannel;

namespace AiGroupChat.UnitTests.Services.ChatHubService;

/// <summary>
/// Tests for ChatHubService member event broadcasts (MemberJoined, MemberLeft, MemberRoleChanged)
/// </summary>
public class BroadcastMemberEventsTests : ChatHubServiceTestBase
{
    #region MemberJoined Tests

    [Fact]
    public async Task BroadcastMemberJoinedAsync_SendsToCorrectGroup()
    {
        // Arrange
        MemberJoinedEvent eventData = CreateMemberJoinedEvent();

        // Act
        await ChatHubService.BroadcastMemberJoinedAsync(TestGroupId, eventData);

        // Assert
        Assert.Equal(GetGroupName(TestGroupId), CapturedGroupName);
    }

    [Fact]
    public async Task BroadcastMemberJoinedAsync_SendsCorrectEventName()
    {
        // Arrange
        MemberJoinedEvent eventData = CreateMemberJoinedEvent();

        // Act
        await ChatHubService.BroadcastMemberJoinedAsync(TestGroupId, eventData);

        // Assert
        Assert.Equal("MemberJoined", CapturedMethodName);
    }

    #endregion

    #region MemberLeft Tests

    [Fact]
    public async Task BroadcastMemberLeftAsync_SendsToCorrectGroup()
    {
        // Arrange
        MemberLeftEvent eventData = CreateMemberLeftEvent();

        // Act
        await ChatHubService.BroadcastMemberLeftAsync(TestGroupId, eventData);

        // Assert
        Assert.Equal(GetGroupName(TestGroupId), CapturedGroupName);
    }

    [Fact]
    public async Task BroadcastMemberLeftAsync_SendsCorrectEventName()
    {
        // Arrange
        MemberLeftEvent eventData = CreateMemberLeftEvent();

        // Act
        await ChatHubService.BroadcastMemberLeftAsync(TestGroupId, eventData);

        // Assert
        Assert.Equal("MemberLeft", CapturedMethodName);
    }

    #endregion

    #region MemberRoleChanged Tests

    [Fact]
    public async Task BroadcastMemberRoleChangedAsync_SendsToCorrectGroup()
    {
        // Arrange
        MemberRoleChangedEvent eventData = CreateMemberRoleChangedEvent();

        // Act
        await ChatHubService.BroadcastMemberRoleChangedAsync(TestGroupId, eventData);

        // Assert
        Assert.Equal(GetGroupName(TestGroupId), CapturedGroupName);
    }

    [Fact]
    public async Task BroadcastMemberRoleChangedAsync_SendsCorrectEventName()
    {
        // Arrange
        MemberRoleChangedEvent eventData = CreateMemberRoleChangedEvent();

        // Act
        await ChatHubService.BroadcastMemberRoleChangedAsync(TestGroupId, eventData);

        // Assert
        Assert.Equal("MemberRoleChanged", CapturedMethodName);
    }

    #endregion

    #region Helper Methods

    private MemberJoinedEvent CreateMemberJoinedEvent()
    {
        return new MemberJoinedEvent
        {
            GroupId = TestGroupId,
            UserId = TestUserId,
            UserName = "newuser",
            DisplayName = "New User",
            Role = "Member",
            JoinedAt = DateTime.UtcNow
        };
    }

    private MemberLeftEvent CreateMemberLeftEvent()
    {
        return new MemberLeftEvent
        {
            GroupId = TestGroupId,
            UserId = TestUserId
        };
    }

    private MemberRoleChangedEvent CreateMemberRoleChangedEvent()
    {
        return new MemberRoleChangedEvent
        {
            GroupId = TestGroupId,
            UserId = TestUserId,
            NewRole = "Admin"
        };
    }

    #endregion
}