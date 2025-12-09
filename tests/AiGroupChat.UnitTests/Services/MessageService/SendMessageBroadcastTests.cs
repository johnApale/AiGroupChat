using AiGroupChat.Application.DTOs.Messages;
using AiGroupChat.Domain.Entities;
using Moq;

namespace AiGroupChat.UnitTests.Services.MessageService;

public class SendMessageBroadcastTests : MessageServiceTestBase
{
    [Fact]
    public async Task SendMessageAsync_BroadcastsMessageToGroup()
    {
        // Arrange
        SendMessageRequest request = new SendMessageRequest { Content = "Hello, world!" };
        Message createdMessage = CreateTestMessage(content: "Hello, world!");

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

        // Act
        MessageResponse result = await MessageService.SendMessageAsync(TestGroupId, request, TestUserId);

        // Assert
        ChatHubServiceMock.Verify(
            x => x.BroadcastMessageAsync(
                TestGroupId,
                It.Is<MessageResponse>(m => m.Content == "Hello, world!" && m.GroupId == TestGroupId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendMessageAsync_BroadcastsCorrectMessageResponse()
    {
        // Arrange
        SendMessageRequest request = new SendMessageRequest { Content = "Test content" };
        Message createdMessage = CreateTestMessage(content: "Test content");

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

        MessageResponse? broadcastedMessage = null;
        ChatHubServiceMock
            .Setup(x => x.BroadcastMessageAsync(It.IsAny<Guid>(), It.IsAny<MessageResponse>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, MessageResponse, CancellationToken>((_, msg, _) => broadcastedMessage = msg);

        // Act
        MessageResponse result = await MessageService.SendMessageAsync(TestGroupId, request, TestUserId);

        // Assert
        Assert.NotNull(broadcastedMessage);
        Assert.Equal(result.Id, broadcastedMessage.Id);
        Assert.Equal(result.Content, broadcastedMessage.Content);
        Assert.Equal(result.GroupId, broadcastedMessage.GroupId);
        Assert.Equal(result.SenderId, broadcastedMessage.SenderId);
    }
}
