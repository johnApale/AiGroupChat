using AiGroupChat.Application.DTOs.Messages;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Moq;

namespace AiGroupChat.UnitTests.Services.AiInvocationService;

public class HandleAsyncAiDisabledTests : AiInvocationServiceTestBase
{
    [Fact]
    public async Task WhenAiDisabled_SavesDisabledMessage()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: false);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        Message? savedMessage = null;

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
                Content = "AI is currently disabled for this group. An admin can enable it in the group settings.",
                AiVisible = true,
                CreatedAt = DateTime.UtcNow
            });

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        VerifyMessageSaved(Times.Once());
        Assert.NotNull(savedMessage);
        Assert.Contains("disabled", savedMessage.Content, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(SenderType.Ai, savedMessage.SenderType);
    }

    [Fact]
    public async Task WhenAiDisabled_BroadcastsMessage()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: false);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        SetupMessageCreation();

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        VerifyMessageBroadcast(group.Id, Times.Once());
    }

    [Fact]
    public async Task WhenAiDisabled_DoesNotCallAiService()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: false);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        SetupMessageCreation();

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        VerifyAiClientCalled(Times.Never());
    }

    [Fact]
    public async Task WhenAiDisabled_DoesNotBroadcastTyping()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: false);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        SetupMessageCreation();

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        VerifyAiTypingBroadcast(group.Id, Times.Never());
    }

    [Fact]
    public async Task WhenAiDisabled_DoesNotBroadcastStoppedTyping()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: false);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        SetupMessageCreation();

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        VerifyAiStoppedTypingBroadcast(group.Id, Times.Never());
    }

    [Fact]
    public async Task WhenAiDisabled_DoesNotFetchContextMessages()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: false);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        SetupMessageCreation();

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        MessageRepositoryMock.Verify(
            x => x.GetAiContextMessagesAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never());
    }

    [Fact]
    public async Task WhenAiDisabled_DoesNotSaveMetadata()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: false);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        SetupMessageCreation();

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        VerifyMetadataSaved(Times.Never());
    }

    [Fact]
    public async Task WhenAiDisabled_MessageHasCorrectAiProviderId()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: false);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        Message? savedMessage = null;

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
                Content = "test",
                AiVisible = true,
                CreatedAt = DateTime.UtcNow
            });

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        Assert.NotNull(savedMessage);
        Assert.Equal(group.AiProvider.Id, savedMessage.AiProviderId);
    }

    [Fact]
    public async Task WhenAiDisabled_MessageIsAiVisible()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: false);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        Message? savedMessage = null;

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
                Content = "test",
                AiVisible = true,
                CreatedAt = DateTime.UtcNow
            });

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        Assert.NotNull(savedMessage);
        Assert.True(savedMessage.AiVisible);
    }
}