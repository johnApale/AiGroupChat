using AiGroupChat.Application.DTOs.AiService;
using AiGroupChat.Application.DTOs.Messages;
using AiGroupChat.Application.DTOs.SignalR.GroupChannel;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Moq;

namespace AiGroupChat.UnitTests.Services.AiInvocationService;

public class HandleAsyncAiEnabledTests : AiInvocationServiceTestBase
{
    [Fact]
    public async Task WhenAiEnabled_BroadcastsAiTyping()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        List<Message> contextMessages = CreateContextMessages(group.Id, 3);

        SetupContextMessages(group.Id, contextMessages);
        SetupAiClientSuccess(CreateTestAiResponse());
        SetupMessageCreation();

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        VerifyAiTypingBroadcast(group.Id, Times.Once());
    }

    [Fact]
    public async Task WhenAiEnabled_AiTypingHasCorrectProviderInfo()
    {
        // Arrange
        AiProvider provider = CreateTestAiProvider("claude", "Anthropic Claude");
        Group group = CreateTestGroup(aiEnabled: true, aiProvider: provider);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        List<Message> contextMessages = CreateContextMessages(group.Id, 3);
        AiTypingEvent? capturedEvent = null;

        SetupContextMessages(group.Id, contextMessages);
        SetupAiClientSuccess(CreateTestAiResponse());
        SetupMessageCreation();

        ChatHubServiceMock
            .Setup(x => x.BroadcastAiTypingAsync(It.IsAny<Guid>(), It.IsAny<AiTypingEvent>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, AiTypingEvent, CancellationToken>((gid, evt, ct) => capturedEvent = evt)
            .Returns(Task.CompletedTask);

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        Assert.NotNull(capturedEvent);
        Assert.Equal(group.Id, capturedEvent.GroupId);
        Assert.Equal(provider.Id, capturedEvent.ProviderId);
        Assert.Equal(provider.DisplayName, capturedEvent.ProviderName);
    }

    [Fact]
    public async Task WhenAiEnabled_FetchesContextMessages()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        List<Message> contextMessages = CreateContextMessages(group.Id, 5);

        SetupContextMessages(group.Id, contextMessages);
        SetupAiClientSuccess(CreateTestAiResponse());
        SetupMessageCreation();

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        MessageRepositoryMock.Verify(
            x => x.GetAiContextMessagesAsync(group.Id, 100, It.IsAny<CancellationToken>()),
            Times.Once());
    }

    [Fact]
    public async Task WhenAiEnabled_CallsAiServiceWithCorrectRequest()
    {
        // Arrange
        AiProvider provider = CreateTestAiProvider();
        Group group = CreateTestGroup(aiEnabled: true, aiProvider: provider);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai what is the weather?");
        List<Message> contextMessages = CreateContextMessages(group.Id, 3);
        AiGenerateRequest? capturedRequest = null;

        SetupContextMessages(group.Id, contextMessages);
        SetupMessageCreation();

        AiClientServiceMock
            .Setup(x => x.GenerateAsync(It.IsAny<AiGenerateRequest>(), It.IsAny<CancellationToken>()))
            .Callback<AiGenerateRequest, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(CreateTestAiResponse());

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal(provider.Name, capturedRequest.Provider);
        Assert.Equal("what is the weather?", capturedRequest.Query);
        Assert.Equal(3, capturedRequest.Context.Count);
        Assert.Equal(provider.DefaultTemperature, capturedRequest.Config.Temperature);
        Assert.Equal(provider.MaxTokensLimit, capturedRequest.Config.MaxTokens);
    }

    [Fact]
    public async Task WhenAiEnabled_SavesAiResponse()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        List<Message> contextMessages = CreateContextMessages(group.Id, 3);
        AiGenerateResponse aiResponse = CreateTestAiResponse("This is the AI response");
        Message? savedMessage = null;

        SetupContextMessages(group.Id, contextMessages);
        SetupAiClientSuccess(aiResponse);

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
                Content = aiResponse.Response,
                AiVisible = true,
                CreatedAt = DateTime.UtcNow
            });

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        VerifyMessageSaved(Times.Once());
        Assert.NotNull(savedMessage);
        Assert.Equal(aiResponse.Response, savedMessage.Content);
        Assert.Equal(SenderType.Ai, savedMessage.SenderType);
        Assert.Null(savedMessage.SenderId);
    }

    [Fact]
    public async Task WhenAiEnabled_SavesResponseMetadata()
    {
        // Arrange
        AiProvider provider = CreateTestAiProvider();
        Group group = CreateTestGroup(aiEnabled: true, aiProvider: provider);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        List<Message> contextMessages = CreateContextMessages(group.Id, 3);
        AiGenerateResponse aiResponse = new AiGenerateResponse
        {
            Response = "Test response",
            Metadata = new AiResponseMetadataDto
            {
                Provider = "gemini",
                Model = "gemini-1.5-pro",
                TokensInput = 150,
                TokensOutput = 75,
                LatencyMs = 500
            }
        };
        AiResponseMetadata? savedMetadata = null;

        SetupContextMessages(group.Id, contextMessages);
        SetupAiClientSuccess(aiResponse);
        SetupMessageCreation();

        AiResponseMetadataRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<AiResponseMetadata>(), It.IsAny<CancellationToken>()))
            .Callback<AiResponseMetadata, CancellationToken>((m, ct) => savedMetadata = m)
            .ReturnsAsync((AiResponseMetadata m, CancellationToken ct) => m);

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        VerifyMetadataSaved(Times.Once());
        Assert.NotNull(savedMetadata);
        Assert.Equal(aiResponse.Metadata.Model, savedMetadata.Model);
        Assert.Equal(aiResponse.Metadata.TokensInput, savedMetadata.TokensInput);
        Assert.Equal(aiResponse.Metadata.TokensOutput, savedMetadata.TokensOutput);
        Assert.Equal(aiResponse.Metadata.LatencyMs, savedMetadata.LatencyMs);
        Assert.Equal(provider.Id, savedMetadata.AiProviderId);
    }

    [Fact]
    public async Task WhenAiEnabled_BroadcastsAiMessage()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        List<Message> contextMessages = CreateContextMessages(group.Id, 3);

        SetupContextMessages(group.Id, contextMessages);
        SetupAiClientSuccess(CreateTestAiResponse());
        SetupMessageCreation();

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        VerifyMessageBroadcast(group.Id, Times.Once());
    }

    [Fact]
    public async Task WhenAiEnabled_BroadcastsAiStoppedTyping()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        List<Message> contextMessages = CreateContextMessages(group.Id, 3);

        SetupContextMessages(group.Id, contextMessages);
        SetupAiClientSuccess(CreateTestAiResponse());
        SetupMessageCreation();

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        VerifyAiStoppedTypingBroadcast(group.Id, Times.Once());
    }

    [Fact]
    public async Task WhenAiEnabled_StripsAiMentionFromQuery()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai what time is it?");
        List<Message> contextMessages = CreateContextMessages(group.Id, 2);
        AiGenerateRequest? capturedRequest = null;

        SetupContextMessages(group.Id, contextMessages);
        SetupMessageCreation();

        AiClientServiceMock
            .Setup(x => x.GenerateAsync(It.IsAny<AiGenerateRequest>(), It.IsAny<CancellationToken>()))
            .Callback<AiGenerateRequest, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(CreateTestAiResponse());

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal("what time is it?", capturedRequest.Query);
        Assert.DoesNotContain("@ai", capturedRequest.Query);
    }

    [Fact]
    public async Task WhenAiMentionOnly_SendsEmptyQuery()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai");
        List<Message> contextMessages = CreateContextMessages(group.Id, 2);
        AiGenerateRequest? capturedRequest = null;

        SetupContextMessages(group.Id, contextMessages);
        SetupMessageCreation();

        AiClientServiceMock
            .Setup(x => x.GenerateAsync(It.IsAny<AiGenerateRequest>(), It.IsAny<CancellationToken>()))
            .Callback<AiGenerateRequest, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(CreateTestAiResponse());

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal(string.Empty, capturedRequest.Query);
    }

    [Fact]
    public async Task WhenAiEnabled_AiResponseHasProviderAsSenderName()
    {
        // Arrange
        AiProvider provider = CreateTestAiProvider("claude", "Anthropic Claude");
        Group group = CreateTestGroup(aiEnabled: true, aiProvider: provider);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        List<Message> contextMessages = CreateContextMessages(group.Id, 2);
        MessageResponse? broadcastedMessage = null;

        SetupContextMessages(group.Id, contextMessages);
        SetupAiClientSuccess(CreateTestAiResponse());
        SetupMessageCreation();

        ChatHubServiceMock
            .Setup(x => x.BroadcastMessageAsync(It.IsAny<Guid>(), It.IsAny<MessageResponse>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, MessageResponse, CancellationToken>((gid, msg, ct) => broadcastedMessage = msg)
            .Returns(Task.CompletedTask);

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        Assert.NotNull(broadcastedMessage);
        Assert.Equal(provider.Name, broadcastedMessage.SenderUserName);
        Assert.Equal(provider.DisplayName, broadcastedMessage.SenderDisplayName);
        Assert.Equal("Ai", broadcastedMessage.SenderType);
    }

    [Fact]
    public async Task WhenAiEnabled_CalculatesCostEstimate()
    {
        // Arrange
        AiProvider provider = new AiProvider
        {
            Id = Guid.NewGuid(),
            Name = "gemini",
            DisplayName = "Google Gemini",
            IsEnabled = true,
            DefaultModel = "gemini-1.5-pro",
            DefaultTemperature = 0.7m,
            MaxTokensLimit = 2000,
            InputTokenCost = 0.001m,  // $0.001 per 1K tokens
            OutputTokenCost = 0.002m, // $0.002 per 1K tokens
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        Group group = CreateTestGroup(aiEnabled: true, aiProvider: provider);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai hello");
        List<Message> contextMessages = CreateContextMessages(group.Id, 2);
        AiGenerateResponse aiResponse = new AiGenerateResponse
        {
            Response = "Test",
            Metadata = new AiResponseMetadataDto
            {
                Provider = "gemini",
                Model = "gemini-1.5-pro",
                TokensInput = 1000,  // 1K tokens
                TokensOutput = 500,  // 0.5K tokens
                LatencyMs = 200
            }
        };
        AiResponseMetadata? savedMetadata = null;

        SetupContextMessages(group.Id, contextMessages);
        SetupAiClientSuccess(aiResponse);
        SetupMessageCreation();

        AiResponseMetadataRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<AiResponseMetadata>(), It.IsAny<CancellationToken>()))
            .Callback<AiResponseMetadata, CancellationToken>((m, ct) => savedMetadata = m)
            .ReturnsAsync((AiResponseMetadata m, CancellationToken ct) => m);

        // Act
        await AiInvocationService.HandleAsync(group, triggerMessage);

        // Assert
        Assert.NotNull(savedMetadata);
        // Input: 1000/1000 * 0.001 = 0.001
        // Output: 500/1000 * 0.002 = 0.001
        // Total: 0.002
        Assert.Equal(0.002m, savedMetadata.CostEstimate);
    }
}