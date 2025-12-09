using AiGroupChat.Application.DTOs.Messages;
using AiGroupChat.Application.Exceptions;
using AiGroupChat.Domain.Entities;
using Moq;

namespace AiGroupChat.UnitTests.Services.MessageService;

public class SendMessageAsyncTests : MessageServiceTestBase
{
    [Fact]
    public async Task WithValidRequest_CreatesMessage()
    {
        // Arrange
        SendMessageRequest request = new SendMessageRequest { Content = "Hello, world!" };

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
            .ReturnsAsync((Guid id, CancellationToken _) => CreateTestMessage(id, "Hello, world!"));

        // Act
        MessageResponse result = await MessageService.SendMessageAsync(TestGroupId, request, TestUserId);

        // Assert
        Assert.Equal("Hello, world!", result.Content);
        Assert.Equal(TestGroupId, result.GroupId);
        Assert.Equal(TestUserId, result.SenderId);
        Assert.Equal("User", result.SenderType);
        MessageRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WithAiMonitoringEnabled_SetsAiVisibleTrue()
    {
        // Arrange
        TestGroup.AiMonitoringEnabled = true;
        SendMessageRequest request = new SendMessageRequest { Content = "Hello with AI!" };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(TestGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestGroup);

        GroupRepositoryMock
            .Setup(x => x.IsMemberAsync(TestGroupId, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Message? capturedMessage = null;
        MessageRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .Callback<Message, CancellationToken>((m, _) => capturedMessage = m)
            .ReturnsAsync((Message m, CancellationToken _) => m);

        MessageRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => CreateTestMessage(id, "Hello with AI!"));

        // Act
        await MessageService.SendMessageAsync(TestGroupId, request, TestUserId);

        // Assert
        Assert.NotNull(capturedMessage);
        Assert.True(capturedMessage!.AiVisible);
    }

    [Fact]
    public async Task WithNonexistentGroup_ThrowsNotFoundException()
    {
        // Arrange
        SendMessageRequest request = new SendMessageRequest { Content = "Hello!" };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(TestGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Group?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            MessageService.SendMessageAsync(TestGroupId, request, TestUserId));
    }

    [Fact]
    public async Task WithNonMember_ThrowsAuthorizationException()
    {
        // Arrange
        SendMessageRequest request = new SendMessageRequest { Content = "Hello!" };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(TestGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestGroup);

        GroupRepositoryMock
            .Setup(x => x.IsMemberAsync(TestGroupId, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<AuthorizationException>(() =>
            MessageService.SendMessageAsync(TestGroupId, request, TestUserId));
    }
}
