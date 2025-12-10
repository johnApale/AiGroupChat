using AiGroupChat.Application.DTOs.Messages;
using Moq;

namespace AiGroupChat.UnitTests.Services.ChatHubService;

/// <summary>
/// Tests for ChatHubService.BroadcastMessageAsync
/// </summary>
public class BroadcastMessageTests : ChatHubServiceTestBase
{
    [Fact]
    public async Task BroadcastMessageAsync_SendsToCorrectGroup()
    {
        // Arrange
        MessageResponse message = CreateTestMessage();

        // Act
        await ChatHubService.BroadcastMessageAsync(TestGroupId, message);

        // Assert
        Assert.Equal(GetGroupName(TestGroupId), CapturedGroupName);
    }

    [Fact]
    public async Task BroadcastMessageAsync_SendsCorrectEventName()
    {
        // Arrange
        MessageResponse message = CreateTestMessage();

        // Act
        await ChatHubService.BroadcastMessageAsync(TestGroupId, message);

        // Assert
        Assert.Equal("MessageReceived", CapturedMethodName);
    }

    [Fact]
    public async Task BroadcastMessageAsync_SendsCorrectPayload()
    {
        // Arrange
        MessageResponse message = CreateTestMessage();

        // Act
        await ChatHubService.BroadcastMessageAsync(TestGroupId, message);

        // Assert
        Assert.NotNull(CapturedArgs);
        Assert.Single(CapturedArgs);
        Assert.Same(message, CapturedArgs[0]);
    }

    private MessageResponse CreateTestMessage()
    {
        return new MessageResponse
        {
            Id = Guid.NewGuid(),
            GroupId = TestGroupId,
            SenderId = TestUserId,
            SenderUserName = "testuser",
            SenderDisplayName = "Test User",
            SenderType = "User",
            Content = "Hello, world!",
            CreatedAt = DateTime.UtcNow
        };
    }
}