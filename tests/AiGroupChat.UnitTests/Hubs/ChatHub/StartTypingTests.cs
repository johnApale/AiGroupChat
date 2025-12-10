using AiGroupChat.Application.DTOs.SignalR.GroupChannel;
using Moq;

namespace AiGroupChat.UnitTests.Hubs.ChatHub;

/// <summary>
/// Tests for the ChatHub.StartTyping method
/// </summary>
public class StartTypingTests : ChatHubTestBase
{
    [Fact]
    public async Task StartTyping_WhenMember_BroadcastsToOthersInGroup()
    {
        // Arrange
        SetupUserIsMember(isMember: true);
        SetupUserExists();

        // Act
        await Hub.StartTyping(TestGroupId);

        // Assert - should broadcast to others in group
        OthersInGroupClientProxyMock.Verify(
            c => c.SendCoreAsync(
                "UserTyping",
                It.Is<object[]>(args => args.Length == 1 && args[0] is UserTypingEvent),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task StartTyping_WhenNotMember_DoesNotBroadcast()
    {
        // Arrange
        SetupUserIsMember(isMember: false);

        // Act
        await Hub.StartTyping(TestGroupId);

        // Assert - should NOT broadcast anything
        OthersInGroupClientProxyMock.Verify(
            c => c.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task StartTyping_BroadcastsCorrectUserInfo()
    {
        // Arrange
        SetupUserIsMember(isMember: true);
        SetupUserExists();

        UserTypingEvent? capturedEvent = null;
        OthersInGroupClientProxyMock
            .Setup(c => c.SendCoreAsync(
                "UserTyping",
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, object[], CancellationToken>((method, args, ct) =>
            {
                capturedEvent = args[0] as UserTypingEvent;
            })
            .Returns(Task.CompletedTask);

        // Act
        await Hub.StartTyping(TestGroupId);

        // Assert
        Assert.NotNull(capturedEvent);
        Assert.Equal(TestGroupId, capturedEvent.GroupId);
        Assert.Equal(TestUserId, capturedEvent.UserId);
        Assert.Equal(TestUserName, capturedEvent.UserName);
        Assert.Equal(TestDisplayName, capturedEvent.DisplayName);
    }

    [Fact]
    public async Task StartTyping_WhenUserNotFound_DoesNotBroadcast()
    {
        // Arrange
        SetupUserIsMember(isMember: true);
        // Don't setup user - UserRepository returns null

        // Act
        await Hub.StartTyping(TestGroupId);

        // Assert - should NOT broadcast anything
        OthersInGroupClientProxyMock.Verify(
            c => c.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}