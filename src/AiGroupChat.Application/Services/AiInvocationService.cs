using AiGroupChat.Application.DTOs.AiService;
using AiGroupChat.Application.DTOs.Messages;
using AiGroupChat.Application.DTOs.SignalR.GroupChannel;
using AiGroupChat.Application.Interfaces;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace AiGroupChat.Application.Services;

public class AiInvocationService : IAiInvocationService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IAiResponseMetadataRepository _aiResponseMetadataRepository;
    private readonly IChatHubService _chatHubService;
    private readonly IAiClientService _aiClientService;
    private readonly ILogger<AiInvocationService> _logger;

    private const int MaxContextMessages = 100;
    private const string AiErrorMessage = "Sorry, I'm having trouble processing your message at the moment. Please try again later.";
    private const string AiDisabledMessage = "AI is currently disabled for this group. An admin can enable it in the group settings.";

    public AiInvocationService(
        IMessageRepository messageRepository,
        IAiResponseMetadataRepository aiResponseMetadataRepository,
        IChatHubService chatHubService,
        IAiClientService aiClientService,
        ILogger<AiInvocationService> logger)
    {
        _messageRepository = messageRepository;
        _aiResponseMetadataRepository = aiResponseMetadataRepository;
        _chatHubService = chatHubService;
        _aiClientService = aiClientService;
        _logger = logger;
    }

    public bool IsAiMentioned(string content)
    {
        string trimmed = content.TrimStart();
        return trimmed.StartsWith("@ai ", StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals("@ai", StringComparison.OrdinalIgnoreCase);
    }

    public async Task HandleAsync(Group group, Message triggerMessage, CancellationToken cancellationToken = default)
    {
        AiProvider aiProvider = group.AiProvider;

        // If AI is disabled, send a message saying so
        if (!group.AiMonitoringEnabled)
        {
            _logger.LogInformation("AI mentioned in group {GroupId} but AI is disabled", group.Id);
            await SaveAndBroadcastAiMessageAsync(group, AiDisabledMessage, aiProvider, null, cancellationToken);
            return;
        }

        // Broadcast AI typing indicator
        AiTypingEvent typingEvent = new AiTypingEvent
        {
            GroupId = group.Id,
            ProviderId = aiProvider.Id,
            ProviderName = aiProvider.DisplayName
        };
        await _chatHubService.BroadcastAiTypingAsync(group.Id, typingEvent, cancellationToken);

        try
        {
            // Get context messages (AI-visible messages)
            List<Message> contextMessages = await _messageRepository.GetAiContextMessagesAsync(
                group.Id,
                MaxContextMessages,
                cancellationToken);

            // Build the request
            string query = StripAiMention(triggerMessage.Content);
            AiGenerateRequest request = BuildAiRequest(aiProvider, contextMessages, query);

            _logger.LogInformation(
                "Invoking AI provider {Provider} for group {GroupId} with {ContextCount} context messages",
                aiProvider.Name,
                group.Id,
                contextMessages.Count);

            // Call AI service
            AiGenerateResponse? aiResponse = await _aiClientService.GenerateAsync(request, cancellationToken);

            // Handle response
            if (aiResponse != null)
            {
                await SaveAndBroadcastAiMessageAsync(group, aiResponse.Response, aiProvider, aiResponse.Metadata, cancellationToken);
            }
            else
            {
                _logger.LogWarning("AI service returned null response for group {GroupId}", group.Id);
                await SaveAndBroadcastAiMessageAsync(group, AiErrorMessage, aiProvider, null, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invoking AI for group {GroupId}", group.Id);
            await SaveAndBroadcastAiMessageAsync(group, AiErrorMessage, aiProvider, null, cancellationToken);
        }
        finally
        {
            // Always broadcast stopped typing
            AiStoppedTypingEvent stoppedTypingEvent = new AiStoppedTypingEvent
            {
                GroupId = group.Id,
                ProviderId = aiProvider.Id
            };
            await _chatHubService.BroadcastAiStoppedTypingAsync(group.Id, stoppedTypingEvent, cancellationToken);
        }
    }

    private static string StripAiMention(string content)
    {
        string trimmed = content.TrimStart();
        if (trimmed.StartsWith("@ai ", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed.Substring(4);
        }
        if (trimmed.Equals("@ai", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }
        return content;
    }

    private static AiGenerateRequest BuildAiRequest(AiProvider provider, List<Message> contextMessages, string query)
    {
        List<AiContextMessage> context = contextMessages.Select(m => new AiContextMessage
        {
            Id = m.Id.ToString(),
            SenderType = m.SenderType.ToString().ToLowerInvariant(),
            SenderName = m.Sender?.DisplayName ?? m.Sender?.UserName ?? "Unknown",
            Content = m.Content,
            CreatedAt = m.CreatedAt
        }).ToList();

        return new AiGenerateRequest
        {
            Provider = provider.Name,
            Context = context,
            Query = query,
            Config = new AiGenerateConfig
            {
                Temperature = provider.DefaultTemperature,
                MaxTokens = provider.MaxTokensLimit
            }
        };
    }

    private async Task SaveAndBroadcastAiMessageAsync(
        Group group,
        string content,
        AiProvider aiProvider,
        AiResponseMetadataDto? metadata,
        CancellationToken cancellationToken)
    {
        DateTime now = DateTime.UtcNow;

        // Create AI message
        Message aiMessage = new Message
        {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            SenderId = null,
            SenderType = SenderType.Ai,
            Content = content,
            AiVisible = true,
            AiProviderId = aiProvider.Id,
            CreatedAt = now
        };

        await _messageRepository.CreateAsync(aiMessage, cancellationToken);

        // Save AI response metadata if provided
        if (metadata != null)
        {
            AiResponseMetadata responseMetadata = new AiResponseMetadata
            {
                Id = Guid.NewGuid(),
                MessageId = aiMessage.Id,
                AiProviderId = aiProvider.Id,
                Model = metadata.Model,
                TokensInput = metadata.TokensInput,
                TokensOutput = metadata.TokensOutput,
                LatencyMs = metadata.LatencyMs,
                CostEstimate = CalculateCost(aiProvider, metadata.TokensInput, metadata.TokensOutput),
                CreatedAt = now
            };

            await _aiResponseMetadataRepository.CreateAsync(responseMetadata, cancellationToken);

            _logger.LogInformation(
                "Saved AI response metadata for message {MessageId}: {TokensInput} input, {TokensOutput} output, {LatencyMs}ms",
                aiMessage.Id,
                metadata.TokensInput,
                metadata.TokensOutput,
                metadata.LatencyMs);
        }

        // Fetch with navigation properties
        Message? savedMessage = await _messageRepository.GetByIdAsync(aiMessage.Id, cancellationToken);
        MessageResponse response = MapToAiResponse(savedMessage!, aiProvider);

        // Broadcast to group
        await _chatHubService.BroadcastMessageAsync(group.Id, response, cancellationToken);
    }

    private static decimal? CalculateCost(AiProvider provider, int tokensInput, int tokensOutput)
    {
        // Cost is per 1K tokens
        decimal inputCost = (tokensInput / 1000m) * provider.InputTokenCost;
        decimal outputCost = (tokensOutput / 1000m) * provider.OutputTokenCost;
        return inputCost + outputCost;
    }

    private static MessageResponse MapToAiResponse(Message message, AiProvider provider)
    {
        return new MessageResponse
        {
            Id = message.Id,
            GroupId = message.GroupId,
            SenderId = null,
            SenderUserName = provider.Name,
            SenderDisplayName = provider.DisplayName,
            SenderType = message.SenderType.ToString(),
            Content = message.Content,
            AttachmentUrl = message.AttachmentUrl,
            AttachmentType = message.AttachmentType,
            AttachmentName = message.AttachmentName,
            CreatedAt = message.CreatedAt
        };
    }
}