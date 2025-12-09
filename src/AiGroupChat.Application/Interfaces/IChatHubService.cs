using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.DTOs.Messages;
using AiGroupChat.Application.DTOs.SignalR.GroupChannel;
using AiGroupChat.Application.DTOs.SignalR.PersonalChannel;

namespace AiGroupChat.Application.Interfaces;

/// <summary>
/// Abstraction for SignalR hub operations, allowing Application layer to broadcast events
/// without depending on SignalR infrastructure.
/// </summary>
public interface IChatHubService
{
    #region Group Channel Events

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

    #endregion

    #region Personal Channel Events

    /// <summary>
    /// Send group activity notification to a user's personal channel.
    /// Used for home page list reordering.
    /// </summary>
    Task SendGroupActivityAsync(string userId, GroupActivityEvent eventData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send new message notification to a user's personal channel.
    /// Used for notification badge/drawer.
    /// </summary>
    Task SendNewMessageNotificationAsync(string userId, NewMessageNotificationEvent eventData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notify a user they were added to a group.
    /// </summary>
    Task SendAddedToGroupAsync(string userId, AddedToGroupEvent eventData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notify a user they were removed from a group.
    /// </summary>
    Task SendRemovedFromGroupAsync(string userId, RemovedFromGroupEvent eventData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notify a user their role in a group changed.
    /// </summary>
    Task SendRoleChangedAsync(string userId, RoleChangedEvent eventData, CancellationToken cancellationToken = default);

    #endregion

    #region Presence Events

    /// <summary>
    /// Broadcast that a user came online to all users who share groups with them.
    /// </summary>
    Task SendUserOnlineAsync(IEnumerable<string> userIds, UserOnlineEvent eventData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Broadcast that a user went offline to all users who share groups with them.
    /// </summary>
    Task SendUserOfflineAsync(IEnumerable<string> userIds, UserOfflineEvent eventData, CancellationToken cancellationToken = default);

    #endregion
}