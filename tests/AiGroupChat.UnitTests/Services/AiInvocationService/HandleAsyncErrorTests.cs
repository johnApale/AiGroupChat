using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Moq;

namespace AiGroupChat.UnitTests.Services.AiInvocationService;

public class HandleAsyncErrorTests : AiInvocationServiceTestBase
{
    [Fact]
    public async Task WhenAiServiceReturnsNull_SavesErrorMessage()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        List<Message> contextMessages = CreateContextMessages(group.Id, 3);
        Message? savedMessage = null;

        SetupContextMessages(group.Id, contextMessages);
        SetupAiClientFailure();

        MessageRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .Callback<Message, CancellationToken>((m, ct) => savedMessage = m)
            .ReturnsAsync((Message m, CancellationToken ct) => m);

        MessageRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken ct) => new Message
            {
                Id = id,
                GroupId = group.Id,
                SenderType = SenderType.Ai,
                Content = "Sorry, I'm having trouble processing your message at the moment. Please try again later.",
                AiVisible = true,
                CreatedAt = DateTime.UtcNow
            });

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        VerifyMessageSaved(Times.Once());
        Assert.NotNull(savedMessage);
        Assert.Contains("trouble", savedMessage.Content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task WhenAiServiceReturnsNull_DoesNotSaveMetadata()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        List<Message> contextMessages = CreateContextMessages(group.Id, 3);

        SetupContextMessages(group.Id, contextMessages);
        SetupAiClientFailure();
        SetupMessageCreation();

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        VerifyMetadataSaved(Times.Never());
    }

    [Fact]
    public async Task WhenAiServiceReturnsNull_StillBroadcastsStoppedTyping()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        List<Message> contextMessages = CreateContextMessages(group.Id, 3);

        SetupContextMessages(group.Id, contextMessages);
        SetupAiClientFailure();
        SetupMessageCreation();

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        VerifyAiStoppedTypingBroadcast(group.Id, Times.Once());
    }

    [Fact]
    public async Task WhenAiServiceThrows_SavesErrorMessage()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        List<Message> contextMessages = CreateContextMessages(group.Id, 3);
        Message? savedMessage = null;

        SetupContextMessages(group.Id, contextMessages);
        SetupAiClientException(new Exception("Unexpected error"));

        MessageRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .Callback<Message, CancellationToken>((m, ct) => savedMessage = m)
            .ReturnsAsync((Message m, CancellationToken ct) => m);

        MessageRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken ct) => new Message
            {
                Id = id,
                GroupId = group.Id,
                SenderType = SenderType.Ai,
                Content = "error",
                AiVisible = true,
                CreatedAt = DateTime.UtcNow
            });

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        VerifyMessageSaved(Times.Once());
        Assert.NotNull(savedMessage);
        Assert.Contains("trouble", savedMessage.Content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task WhenAiServiceThrows_StillBroadcastsStoppedTyping()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        List<Message> contextMessages = CreateContextMessages(group.Id, 3);

        SetupContextMessages(group.Id, contextMessages);
        SetupAiClientException(new Exception("Unexpected error"));
        SetupMessageCreation();

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        VerifyAiStoppedTypingBroadcast(group.Id, Times.Once());
    }

    [Fact]
    public async Task WhenAiServiceThrowsHttpRequestException_SavesErrorMessage()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        List<Message> contextMessages = CreateContextMessages(group.Id, 3);
        Message? savedMessage = null;

        SetupContextMessages(group.Id, contextMessages);
        SetupAiClientException(new HttpRequestException("Connection refused"));

        MessageRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .Callback<Message, CancellationToken>((m, ct) => savedMessage = m)
            .ReturnsAsync((Message m, CancellationToken ct) => m);

        MessageRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken ct) => new Message
            {
                Id = id,
                GroupId = group.Id,
                SenderType = SenderType.Ai,
                Content = "error",
                AiVisible = true,
                CreatedAt = DateTime.UtcNow
            });

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        VerifyMessageSaved(Times.Once());
        Assert.NotNull(savedMessage);
        Assert.Contains("trouble", savedMessage.Content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task WhenAiServiceThrowsTaskCanceledException_SavesErrorMessage()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        List<Message> contextMessages = CreateContextMessages(group.Id, 3);
        Message? savedMessage = null;

        SetupContextMessages(group.Id, contextMessages);
        SetupAiClientException(new TaskCanceledException("Request timed out"));

        MessageRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .Callback<Message, CancellationToken>((m, ct) => savedMessage = m)
            .ReturnsAsync((Message m, CancellationToken ct) => m);

        MessageRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken ct) => new Message
            {
                Id = id,
                GroupId = group.Id,
                SenderType = SenderType.Ai,
                Content = "error",
                AiVisible = true,
                CreatedAt = DateTime.UtcNow
            });

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        VerifyMessageSaved(Times.Once());
        Assert.NotNull(savedMessage);
    }

    [Fact]
    public async Task WhenAiServiceThrows_DoesNotSaveMetadata()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        List<Message> contextMessages = CreateContextMessages(group.Id, 3);

        SetupContextMessages(group.Id, contextMessages);
        SetupAiClientException(new Exception("Error"));
        SetupMessageCreation();

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        VerifyMetadataSaved(Times.Never());
    }

    [Fact]
    public async Task WhenAiServiceThrows_BroadcastsAiTypingBeforeError()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        List<Message> contextMessages = CreateContextMessages(group.Id, 3);

        SetupContextMessages(group.Id, contextMessages);
        SetupAiClientException(new Exception("Error"));
        SetupMessageCreation();

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        VerifyAiTypingBroadcast(group.Id, Times.Once());
    }

    [Fact]
    public async Task WhenAiServiceThrows_BroadcastsErrorMessage()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        List<Message> contextMessages = CreateContextMessages(group.Id, 3);

        SetupContextMessages(group.Id, contextMessages);
        SetupAiClientException(new Exception("Error"));
        SetupMessageCreation();

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        VerifyMessageBroadcast(group.Id, Times.Once());
    }

    [Fact]
    public async Task WhenContextFetchFails_SavesErrorMessage()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        Message? savedMessage = null;

        MessageRepositoryMock
            .Setup(x => x.GetAiContextMessagesAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        MessageRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .Callback<Message, CancellationToken>((m, ct) => savedMessage = m)
            .ReturnsAsync((Message m, CancellationToken ct) => m);

        MessageRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken ct) => new Message
            {
                Id = id,
                GroupId = group.Id,
                SenderType = SenderType.Ai,
                Content = "error",
                AiVisible = true,
                CreatedAt = DateTime.UtcNow
            });

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        VerifyMessageSaved(Times.Once());
        Assert.NotNull(savedMessage);
        Assert.Contains("trouble", savedMessage.Content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task WhenContextFetchFails_StillBroadcastsStoppedTyping()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");

        MessageRepositoryMock
            .Setup(x => x.GetAiContextMessagesAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        SetupMessageCreation();

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        VerifyAiStoppedTypingBroadcast(group.Id, Times.Once());
    }

    [Fact]
    public async Task WhenErrorOccurs_AiClientNotCalled()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");

        MessageRepositoryMock
            .Setup(x => x.GetAiContextMessagesAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        SetupMessageCreation();

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        VerifyAiClientCalled(Times.Never());
    }
}