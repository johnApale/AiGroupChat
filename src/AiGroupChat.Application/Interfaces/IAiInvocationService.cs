using AiGroupChat.Domain.Entities;

namespace AiGroupChat.Application.Interfaces;

/// <summary>
/// Handles AI invocation when users @mention AI in messages
/// </summary>
public interface IAiInvocationService
{
    /// <summary>
    /// Check if the message content contains an AI mention at the start
    /// </summary>
    bool IsAiMentioned(string content);

    /// <summary>
    /// Handle AI invocation for a message that mentioned AI.
    /// Broadcasts typing indicators, calls AI service, and saves/broadcasts response.
    /// </summary>
    Task HandleAsync(Group group, Message triggerMessage, CancellationToken cancellationToken = default);
}