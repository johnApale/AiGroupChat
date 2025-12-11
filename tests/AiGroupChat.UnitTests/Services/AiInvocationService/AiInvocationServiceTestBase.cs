using AiGroupChat.Application.DTOs.AiService;
using AiGroupChat.Application.DTOs.Messages;
using AiGroupChat.Application.DTOs.SignalR.GroupChannel;
using AiGroupChat.Application.Interfaces;
using AiGroupChat.Application.Services;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace AiGroupChat.UnitTests.Services.AiInvocationService;

public abstract class AiInvocationServiceTestBase
{
    protected readonly Mock<IMessageRepository> MessageRepositoryMock;
    protected readonly Mock<IAiResponseMetadataRepository> AiResponseMetadataRepositoryMock;
    protected readonly Mock<IChatHubService> ChatHubServiceMock;
    protected readonly Mock<IAiClientService> AiClientServiceMock;
    protected readonly Mock<ILogger<Application.Services.AiInvocationService>> LoggerMock;
    protected readonly Application.Services.AiInvocationService AiInvocationService;

    protected AiInvocationServiceTestBase()
    {
        MessageRepositoryMock = new Mock<IMessageRepository>();
        AiResponseMetadataRepositoryMock = new Mock<IAiResponseMetadataRepository>();
        ChatHubServiceMock = new Mock<IChatHubService>();
        AiClientServiceMock = new Mock<IAiClientService>();
        LoggerMock = new Mock<ILogger<Application.Services.AiInvocationService>>();

        AiInvocationService = new Application.Services.AiInvocationService(
            MessageRepositoryMock.Object,
            AiResponseMetadataRepositoryMock.Object,
            ChatHubServiceMock.Object,
            AiClientServiceMock.Object,
            LoggerMock.Object
        );
    }

    /// <summary>
    /// Creates a test AI provider with default values
    /// </summary>
    protected AiProvider CreateTestAiProvider(
        string name = "gemini",
        string displayName = "Google Gemini",
        decimal temperature = 0.7m,
        int maxTokens = 2000)
    {
        return new AiProvider
        {
            Id = Guid.NewGuid(),
            Name = name,
            DisplayName = displayName,
            IsEnabled = true,
            DefaultModel = "gemini-1.5-pro",
            DefaultTemperature = temperature,
            MaxTokensLimit = maxTokens,
            InputTokenCost = 0.001m,
            OutputTokenCost = 0.002m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test group with the specified AI settings
    /// </summary>
    protected Group CreateTestGroup(bool aiEnabled = true, AiProvider? aiProvider = null)
    {
        AiProvider provider = aiProvider ?? CreateTestAiProvider();
        return new Group
        {
            Id = Guid.NewGuid(),
            Name = "Test Group",
            CreatedById = "user-123",
            AiMonitoringEnabled = aiEnabled,
            AiProviderId = provider.Id,
            AiProvider = provider,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test user message that triggers AI
    /// </summary>
    protected Message CreateTriggerMessage(Guid groupId, string content = "@ai Hello!")
    {
        User sender = CreateTestUser();
        return new Message
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            SenderId = sender.Id,
            Sender = sender,
            SenderType = SenderType.User,
            Content = content,
            AiVisible = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test user
    /// </summary>
    protected User CreateTestUser(string displayName = "Test User", string userName = "testuser")
    {
        return new User
        {
            Id = Guid.NewGuid().ToString(),
            UserName = userName,
            DisplayName = displayName,
            Email = "test@example.com"
        };
    }

    /// <summary>
    /// Creates a list of context messages for AI
    /// </summary>
    protected List<Message> CreateContextMessages(Guid groupId, int count = 5)
    {
        List<Message> messages = new();
        DateTime baseTime = DateTime.UtcNow.AddMinutes(-count);

        for (int i = 0; i < count; i++)
        {
            User sender = CreateTestUser($"User {i}", $"user{i}");
            messages.Add(new Message
            {
                Id = Guid.NewGuid(),
                GroupId = groupId,
                SenderId = sender.Id,
                Sender = sender,
                SenderType = i % 3 == 0 ? SenderType.Ai : SenderType.User,
                Content = $"Message content {i}",
                AiVisible = true,
                CreatedAt = baseTime.AddMinutes(i)
            });
        }

        return messages;
    }

    /// <summary>
    /// Creates a successful AI generate response
    /// </summary>
    protected AiGenerateResponse CreateTestAiResponse(string responseText = "Hello! I'm here to help.")
    {
        return new AiGenerateResponse
        {
            Response = responseText,
            Metadata = new AiResponseMetadataDto
            {
                Provider = "gemini",
                Model = "gemini-1.5-pro",
                TokensInput = 100,
                TokensOutput = 50,
                LatencyMs = 250
            },
            Attachment = null
        };
    }

    /// <summary>
    /// Sets up message repository to return context messages
    /// </summary>
    protected void SetupContextMessages(Guid groupId, List<Message> messages)
    {
        MessageRepositoryMock
            .Setup(x => x.GetAiContextMessagesAsync(groupId, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(messages);
    }

    /// <summary>
    /// Sets up message repository to return the created message when GetByIdAsync is called
    /// </summary>
    protected void SetupMessageCreation()
    {
        MessageRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Message m, CancellationToken ct) => m);

        MessageRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken ct) => new Message
            {
                Id = id,
                GroupId = Guid.NewGuid(),
                SenderType = SenderType.Ai,
                Content = "Test response",
                AiVisible = true,
                CreatedAt = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Sets up AI client service to return a successful response
    /// </summary>
    protected void SetupAiClientSuccess(AiGenerateResponse response)
    {
        AiClientServiceMock
            .Setup(x => x.GenerateAsync(It.IsAny<AiGenerateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
    }

    /// <summary>
    /// Sets up AI client service to return null (failure)
    /// </summary>
    protected void SetupAiClientFailure()
    {
        AiClientServiceMock
            .Setup(x => x.GenerateAsync(It.IsAny<AiGenerateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AiGenerateResponse?)null);
    }

    /// <summary>
    /// Sets up AI client service to throw an exception
    /// </summary>
    protected void SetupAiClientException(Exception exception)
    {
        AiClientServiceMock
            .Setup(x => x.GenerateAsync(It.IsAny<AiGenerateRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
    }

    /// <summary>
    /// Verifies that AiTyping was broadcast
    /// </summary>
    protected void VerifyAiTypingBroadcast(Guid groupId, Times times)
    {
        ChatHubServiceMock.Verify(
            x => x.BroadcastAiTypingAsync(
                groupId,
                It.IsAny<AiTypingEvent>(),
                It.IsAny<CancellationToken>()),
            times);
    }

    /// <summary>
    /// Verifies that AiStoppedTyping was broadcast
    /// </summary>
    protected void VerifyAiStoppedTypingBroadcast(Guid groupId, Times times)
    {
        ChatHubServiceMock.Verify(
            x => x.BroadcastAiStoppedTypingAsync(
                groupId,
                It.IsAny<AiStoppedTypingEvent>(),
                It.IsAny<CancellationToken>()),
            times);
    }

    /// <summary>
    /// Verifies that a message was broadcast to the group
    /// </summary>
    protected void VerifyMessageBroadcast(Guid groupId, Times times)
    {
        ChatHubServiceMock.Verify(
            x => x.BroadcastMessageAsync(
                groupId,
                It.IsAny<MessageResponse>(),
                It.IsAny<CancellationToken>()),
            times);
    }

    /// <summary>
    /// Verifies that a message was saved
    /// </summary>
    protected void VerifyMessageSaved(Times times)
    {
        MessageRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()),
            times);
    }

    /// <summary>
    /// Verifies that AI metadata was saved
    /// </summary>
    protected void VerifyMetadataSaved(Times times)
    {
        AiResponseMetadataRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<AiResponseMetadata>(), It.IsAny<CancellationToken>()),
            times);
    }

    /// <summary>
    /// Verifies that the AI client was called
    /// </summary>
    protected void VerifyAiClientCalled(Times times)
    {
        AiClientServiceMock.Verify(
            x => x.GenerateAsync(It.IsAny<AiGenerateRequest>(), It.IsAny<CancellationToken>()),
            times);
    }
}