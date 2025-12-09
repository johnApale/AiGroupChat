using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.DTOs.Messages;
using AiGroupChat.Application.DTOs.SignalR;

namespace AiGroupChat.Application.Interfaces;

/// <summary>
/// Abstraction for SignalR hub operations, allowing Application layer to broadcast events
/// without depending on SignalR infrastructure.
/// </summary>
public interface IChatHubService
{
    /// <summary>
    /// Broadcast a new message to all members of a group
    /// </summary>
    Task BroadcastMessageAsync(Guid groupId, MessageResponse message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Broadcast AI settings change to all members of a group
    /// </summary>
    Task BroadcastAiSettingsChangedAsync(Guid groupId, AiSettingsChangedEvent settings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Broadcast that a member was added to a group
    /// </summary>
    Task BroadcastMemberAddedAsync(Guid groupId, GroupMemberResponse member, CancellationToken cancellationToken = default);

    /// <summary>
    /// Broadcast that a member was removed from a group
    /// </summary>
    Task BroadcastMemberRemovedAsync(Guid groupId, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Broadcast that a member's role was changed
    /// </summary>
    Task BroadcastMemberRoleChangedAsync(Guid groupId, string userId, string newRole, CancellationToken cancellationToken = default);

    /// <summary>
    /// Broadcast that a user started typing in a group
    /// </summary>
    Task BroadcastUserTypingAsync(Guid groupId, UserTypingEvent typingEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Broadcast that a user stopped typing in a group
    /// </summary>
    Task BroadcastUserStoppedTypingAsync(Guid groupId, string userId, CancellationToken cancellationToken = default);
}
