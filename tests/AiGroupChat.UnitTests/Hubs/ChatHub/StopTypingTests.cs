using AiGroupChat.Application.DTOs.SignalR.GroupChannel;
using Moq;

namespace AiGroupChat.UnitTests.Hubs.ChatHub;

/// <summary>
/// Tests for the ChatHub.StopTyping method
/// </summary>
public class StopTypingTests : ChatHubTestBase
{
    [Fact]
    public async Task StopTyping_WhenMember_BroadcastsToOthersInGroup()
    {
        // Arrange
        SetupUserIsMember(isMember: true);

        // Act
        await Hub.StopTyping(TestGroupId);

        // Assert - should broadcast to others in group
        OthersInGroupClientProxyMock.Verify(
            c => c.SendCoreAsync(
                "UserStoppedTyping",
                It.Is<object[]>(args => args.Length == 1 && args[0] is UserStoppedTypingEvent),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task StopTyping_WhenNotMember_DoesNotBroadcast()
    {
        // Arrange
        SetupUserIsMember(isMember: false);

        // Act
        await Hub.StopTyping(TestGroupId);

        // Assert - should NOT broadcast anything
        OthersInGroupClientProxyMock.Verify(
            c => c.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task StopTyping_BroadcastsCorrectData()
    {
        // Arrange
        SetupUserIsMember(isMember: true);

        UserStoppedTypingEvent? capturedEvent = null;
        OthersInGroupClientProxyMock
            .Setup(c => c.SendCoreAsync(
                "UserStoppedTyping",
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, object[], CancellationToken>((method, args, ct) =>
            {
                capturedEvent = args[0] as UserStoppedTypingEvent;
            })
            .Returns(Task.CompletedTask);

        // Act
        await Hub.StopTyping(TestGroupId);

        // Assert
        Assert.NotNull(capturedEvent);
        Assert.Equal(TestGroupId, capturedEvent.GroupId);
        Assert.Equal(TestUserId, capturedEvent.UserId);
    }
}