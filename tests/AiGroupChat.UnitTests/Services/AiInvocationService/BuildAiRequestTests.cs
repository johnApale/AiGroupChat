using AiGroupChat.Application.DTOs.AiService;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Moq;

namespace AiGroupChat.UnitTests.Services.AiInvocationService;

public class BuildAiRequestTests : AiInvocationServiceTestBase
{
    [Fact]
    public async Task BuildsRequestWithCorrectProvider()
    {
        // Arrange
        AiProvider provider = CreateTestAiProvider("claude", "Anthropic Claude");
        Group group = CreateTestGroup(aiEnabled: true, aiProvider: provider);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai test");
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
        Assert.Equal("claude", capturedRequest.Provider);
    }

    [Fact]
    public async Task BuildsRequestWithCorrectConfig()
    {
        // Arrange
        AiProvider provider = CreateTestAiProvider(temperature: 0.9m, maxTokens: 4000);
        Group group = CreateTestGroup(aiEnabled: true, aiProvider: provider);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai test");
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
        Assert.Equal(0.9m, capturedRequest.Config.Temperature);
        Assert.Equal(4000, capturedRequest.Config.MaxTokens);
    }

    [Fact]
    public async Task BuildsContextInChronologicalOrder()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai test");
        
        DateTime baseTime = DateTime.UtcNow;
        List<Message> contextMessages = new()
        {
            new Message
            {
                Id = Guid.NewGuid(),
                GroupId = group.Id,
                SenderId = "user-1",
                Sender = CreateTestUser("User 1", "user1"),
                SenderType = SenderType.User,
                Content = "First message",
                AiVisible = true,
                CreatedAt = baseTime.AddMinutes(-3)
            },
            new Message
            {
                Id = Guid.NewGuid(),
                GroupId = group.Id,
                SenderId = "user-2",
                Sender = CreateTestUser("User 2", "user2"),
                SenderType = SenderType.User,
                Content = "Second message",
                AiVisible = true,
                CreatedAt = baseTime.AddMinutes(-2)
            },
            new Message
            {
                Id = Guid.NewGuid(),
                GroupId = group.Id,
                SenderId = null,
                Sender = null,
                SenderType = SenderType.Ai,
                Content = "Third message",
                AiVisible = true,
                CreatedAt = baseTime.AddMinutes(-1)
            }
        };

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
        Assert.Equal(3, capturedRequest.Context.Count);
        Assert.Equal("First message", capturedRequest.Context[0].Content);
        Assert.Equal("Second message", capturedRequest.Context[1].Content);
        Assert.Equal("Third message", capturedRequest.Context[2].Content);
    }

    [Fact]
    public async Task IncludesUserAndAiMessages()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai test");
        
        List<Message> contextMessages = new()
        {
            new Message
            {
                Id = Guid.NewGuid(),
                GroupId = group.Id,
                SenderId = "user-1",
                Sender = CreateTestUser("User 1", "user1"),
                SenderType = SenderType.User,
                Content = "User message",
                AiVisible = true,
                CreatedAt = DateTime.UtcNow.AddMinutes(-2)
            },
            new Message
            {
                Id = Guid.NewGuid(),
                GroupId = group.Id,
                SenderId = null,
                Sender = null,
                SenderType = SenderType.Ai,
                Content = "AI message",
                AiVisible = true,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            }
        };

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
        Assert.Equal(2, capturedRequest.Context.Count);
        Assert.Equal("user", capturedRequest.Context[0].SenderType);
        Assert.Equal("ai", capturedRequest.Context[1].SenderType);
    }

    [Fact]
    public async Task UsesDisplayNameForSenderName()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai test");
        
        User user = new User
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "jdoe",
            DisplayName = "John Doe",
            Email = "john@example.com"
        };
        
        List<Message> contextMessages = new()
        {
            new Message
            {
                Id = Guid.NewGuid(),
                GroupId = group.Id,
                SenderId = user.Id,
                Sender = user,
                SenderType = SenderType.User,
                Content = "Hello",
                AiVisible = true,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            }
        };

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
        Assert.Single(capturedRequest.Context);
        Assert.Equal("John Doe", capturedRequest.Context[0].SenderName);
    }

    [Fact]
    public async Task WithEmptyDisplayName_UsesEmptyString()
    {
        // Arrange
        // Note: The code uses null-coalescing (??), so empty string is NOT treated as "no display name"
        // This test verifies the current behavior. If we want to fall back on empty string,
        // the production code would need to be updated.
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai test");
        
        User user = new User
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "jdoe",
            DisplayName = string.Empty,
            Email = "john@example.com"
        };
        
        List<Message> contextMessages = new()
        {
            new Message
            {
                Id = Guid.NewGuid(),
                GroupId = group.Id,
                SenderId = user.Id,
                Sender = user,
                SenderType = SenderType.User,
                Content = "Hello",
                AiVisible = true,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            }
        };

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
        Assert.Single(capturedRequest.Context);
        // Empty string is used as-is (null-coalescing doesn't treat empty string as null)
        Assert.Equal(string.Empty, capturedRequest.Context[0].SenderName);
    }

    [Fact]
    public async Task FallsBackToUnknownIfNoSender()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai test");
        
        List<Message> contextMessages = new()
        {
            new Message
            {
                Id = Guid.NewGuid(),
                GroupId = group.Id,
                SenderId = null,
                Sender = null,
                SenderType = SenderType.Ai,
                Content = "AI response",
                AiVisible = true,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            }
        };

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
        Assert.Single(capturedRequest.Context);
        Assert.Equal("Unknown", capturedRequest.Context[0].SenderName);
    }

    [Fact]
    public async Task IncludesMessageIdInContext()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai test");
        
        Guid messageId = Guid.NewGuid();
        List<Message> contextMessages = new()
        {
            new Message
            {
                Id = messageId,
                GroupId = group.Id,
                SenderId = "user-1",
                Sender = CreateTestUser("User", "user"),
                SenderType = SenderType.User,
                Content = "Hello",
                AiVisible = true,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            }
        };

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
        Assert.Single(capturedRequest.Context);
        Assert.Equal(messageId.ToString(), capturedRequest.Context[0].Id);
    }

    [Fact]
    public async Task IncludesCreatedAtInContext()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai test");
        
        DateTime messageTime = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        List<Message> contextMessages = new()
        {
            new Message
            {
                Id = Guid.NewGuid(),
                GroupId = group.Id,
                SenderId = "user-1",
                Sender = CreateTestUser("User", "user"),
                SenderType = SenderType.User,
                Content = "Hello",
                AiVisible = true,
                CreatedAt = messageTime
            }
        };

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
        Assert.Single(capturedRequest.Context);
        Assert.Equal(messageTime, capturedRequest.Context[0].CreatedAt);
    }

    [Fact]
    public async Task WithEmptyContext_StillCallsAiService()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai test");
        List<Message> contextMessages = new();
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
        Assert.Empty(capturedRequest.Context);
        VerifyAiClientCalled(Times.Once());
    }

    [Fact]
    public async Task SenderTypeLowercase()
    {
        // Arrange
        Group group = CreateTestGroup(aiEnabled: true);
        Message triggerMessage = CreateTriggerMessage(group.Id, "@ai test");
        
        List<Message> contextMessages = new()
        {
            new Message
            {
                Id = Guid.NewGuid(),
                GroupId = group.Id,
                SenderId = "user-1",
                Sender = CreateTestUser("User", "user"),
                SenderType = SenderType.User,
                Content = "User message",
                AiVisible = true,
                CreatedAt = DateTime.UtcNow.AddMinutes(-2)
            },
            new Message
            {
                Id = Guid.NewGuid(),
                GroupId = group.Id,
                SenderId = null,
                Sender = null,
                SenderType = SenderType.Ai,
                Content = "AI message",
                AiVisible = true,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            }
        };

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
        Assert.Equal("user", capturedRequest.Context[0].SenderType);
        Assert.Equal("ai", capturedRequest.Context[1].SenderType);
    }
}