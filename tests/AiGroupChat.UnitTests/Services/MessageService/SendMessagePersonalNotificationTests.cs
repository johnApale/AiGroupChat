using AiGroupChat.Application.DTOs.Messages;
using AiGroupChat.Application.DTOs.SignalR.PersonalChannel;
using AiGroupChat.Domain.Entities;
using Moq;

namespace AiGroupChat.UnitTests.Services.MessageService;

public class SendMessagePersonalNotificationTests : MessageServiceTestBase
{
    private readonly string _otherUserId1 = "other-user-1";
    private readonly string _otherUserId2 = "other-user-2";

    [Fact]
    public async Task SendMessageAsync_SendsGroupActivityToOtherMembers()
    {
        // Arrange
        SendMessageRequest request = new SendMessageRequest { Content = "Hello everyone!" };
        Message createdMessage = CreateTestMessage(content: "Hello everyone!");

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(TestGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestGroup);

        GroupRepositoryMock
            .Setup(x => x.IsMemberAsync(TestGroupId, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        MessageRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Message m, CancellationToken _) => m);

        MessageRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdMessage);

        // Return list including sender and other members
        GroupMemberRepositoryMock
            .Setup(x => x.GetGroupMemberIdsAsync(TestGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { TestUserId, _otherUserId1, _otherUserId2 });

        // Act
        await MessageService.SendMessageAsync(TestGroupId, request, TestUserId);

        // Assert - should send to other members, not to sender
        ChatHubServiceMock.Verify(
            x => x.SendGroupActivityAsync(
                _otherUserId1,
                It.Is<GroupActivityEvent>(e => 
                    e.GroupId == TestGroupId &&
                    e.GroupName == TestGroup.Name &&
                    e.ActivityType == "NewMessage"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        ChatHubServiceMock.Verify(
            x => x.SendGroupActivityAsync(
                _otherUserId2,
                It.IsAny<GroupActivityEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        // Verify sender does NOT receive notification
        ChatHubServiceMock.Verify(
            x => x.SendGroupActivityAsync(
                TestUserId,
                It.IsAny<GroupActivityEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SendMessageAsync_SendsNewMessageNotificationToOtherMembers()
    {
        // Arrange
        SendMessageRequest request = new SendMessageRequest { Content = "Important update!" };
        Message createdMessage = CreateTestMessage(content: "Important update!");

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(TestGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestGroup);

        GroupRepositoryMock
            .Setup(x => x.IsMemberAsync(TestGroupId, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        MessageRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Message m, CancellationToken _) => m);

        MessageRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdMessage);

        GroupMemberRepositoryMock
            .Setup(x => x.GetGroupMemberIdsAsync(TestGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { TestUserId, _otherUserId1 });

        // Act
        await MessageService.SendMessageAsync(TestGroupId, request, TestUserId);

        // Assert
        ChatHubServiceMock.Verify(
            x => x.SendNewMessageNotificationAsync(
                _otherUserId1,
                It.Is<NewMessageNotificationEvent>(e => 
                    e.GroupId == TestGroupId &&
                    e.GroupName == TestGroup.Name &&
                    e.SenderName == "Test User" &&
                    e.Preview == "Important update!"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendMessageAsync_TruncatesLongMessagePreview()
    {
        // Arrange
        string longMessage = new string('x', 100); // 100 characters
        SendMessageRequest request = new SendMessageRequest { Content = longMessage };
        Message createdMessage = CreateTestMessage(content: longMessage);

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(TestGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestGroup);

        GroupRepositoryMock
            .Setup(x => x.IsMemberAsync(TestGroupId, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        MessageRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Message m, CancellationToken _) => m);

        MessageRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdMessage);

        GroupMemberRepositoryMock
            .Setup(x => x.GetGroupMemberIdsAsync(TestGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { TestUserId, _otherUserId1 });

        NewMessageNotificationEvent? capturedEvent = null;
        ChatHubServiceMock
            .Setup(x => x.SendNewMessageNotificationAsync(_otherUserId1, It.IsAny<NewMessageNotificationEvent>(), It.IsAny<CancellationToken>()))
            .Callback<string, NewMessageNotificationEvent, CancellationToken>((_, e, _) => capturedEvent = e);

        // Act
        await MessageService.SendMessageAsync(TestGroupId, request, TestUserId);

        // Assert - preview should be truncated to 50 chars + "..."
        Assert.NotNull(capturedEvent);
        Assert.Equal(53, capturedEvent.Preview.Length); // 50 + "..."
        Assert.EndsWith("...", capturedEvent.Preview);
    }

    [Fact]
    public async Task SendMessageAsync_DoesNotSendPersonalNotifications_WhenNoOtherMembers()
    {
        // Arrange
        SendMessageRequest request = new SendMessageRequest { Content = "Talking to myself" };
        Message createdMessage = CreateTestMessage(content: "Talking to myself");

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(TestGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestGroup);

        GroupRepositoryMock
            .Setup(x => x.IsMemberAsync(TestGroupId, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        MessageRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Message m, CancellationToken _) => m);

        MessageRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdMessage);

        // Only the sender is a member
        GroupMemberRepositoryMock
            .Setup(x => x.GetGroupMemberIdsAsync(TestGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { TestUserId });

        // Act
        await MessageService.SendMessageAsync(TestGroupId, request, TestUserId);

        // Assert - no personal channel notifications should be sent
        ChatHubServiceMock.Verify(
            x => x.SendGroupActivityAsync(
                It.IsAny<string>(),
                It.IsAny<GroupActivityEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        ChatHubServiceMock.Verify(
            x => x.SendNewMessageNotificationAsync(
                It.IsAny<string>(),
                It.IsAny<NewMessageNotificationEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}