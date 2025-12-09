using AiGroupChat.API.Hubs;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.DTOs.Messages;
using AiGroupChat.Application.DTOs.SignalR.GroupChannel;
using AiGroupChat.Application.DTOs.SignalR.PersonalChannel;
using AiGroupChat.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AiGroupChat.API.Services;

public class ChatHubService : IChatHubService
{
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ILogger<ChatHubService> _logger;

    public ChatHubService(IHubContext<ChatHub> hubContext, ILogger<ChatHubService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task BroadcastMessageAsync(Guid groupId, MessageResponse message, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group(GetGroupName(groupId))
            .SendAsync("MessageReceived", message, cancellationToken);
    }

    public async Task BroadcastAiSettingsChangedAsync(Guid groupId, AiSettingsChangedEvent settings, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group(GetGroupName(groupId))
            .SendAsync("AiSettingsChanged", settings, cancellationToken);
    }

    public async Task BroadcastMemberAddedAsync(Guid groupId, GroupMemberResponse member, CancellationToken cancellationToken = default)
    {
        MemberAddedEvent memberEvent = new MemberAddedEvent
        {
            GroupId = groupId,
            Member = member
        };

        await _hubContext.Clients
            .Group(GetGroupName(groupId))
            .SendAsync("MemberAdded", memberEvent, cancellationToken);
    }

    public async Task BroadcastMemberRemovedAsync(Guid groupId, string userId, CancellationToken cancellationToken = default)
    {
        MemberRemovedEvent memberEvent = new MemberRemovedEvent
        {
            GroupId = groupId,
            UserId = userId
        };

        await _hubContext.Clients
            .Group(GetGroupName(groupId))
            .SendAsync("MemberRemoved", memberEvent, cancellationToken);
    }

    public async Task BroadcastMemberRoleChangedAsync(Guid groupId, string userId, string newRole, CancellationToken cancellationToken = default)
    {
        MemberRoleChangedEvent roleEvent = new MemberRoleChangedEvent
        {
            GroupId = groupId,
            UserId = userId,
            NewRole = newRole
        };

        await _hubContext.Clients
            .Group(GetGroupName(groupId))
            .SendAsync("MemberRoleChanged", roleEvent, cancellationToken);
    }

    public async Task BroadcastUserTypingAsync(Guid groupId, UserTypingEvent typingEvent, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group(GetGroupName(groupId))
            .SendAsync("UserTyping", typingEvent, cancellationToken);
    }

    public async Task BroadcastUserStoppedTypingAsync(Guid groupId, string userId, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group(GetGroupName(groupId))
            .SendAsync("UserStoppedTyping", groupId, userId, cancellationToken);
    }

    #region Personal Channel Events

    public async Task SendGroupActivityAsync(string userId, GroupActivityEvent eventData, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group(GetPersonalChannelName(userId))
            .SendAsync("GroupActivity", eventData, cancellationToken);
    }

    public async Task SendNewMessageNotificationAsync(string userId, NewMessageNotificationEvent eventData, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group(GetPersonalChannelName(userId))
            .SendAsync("NewMessageNotification", eventData, cancellationToken);
    }

    public async Task SendAddedToGroupAsync(string userId, AddedToGroupEvent eventData, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group(GetPersonalChannelName(userId))
            .SendAsync("AddedToGroup", eventData, cancellationToken);
    }

    public async Task SendRemovedFromGroupAsync(string userId, RemovedFromGroupEvent eventData, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group(GetPersonalChannelName(userId))
            .SendAsync("RemovedFromGroup", eventData, cancellationToken);
    }

    public async Task SendRoleChangedAsync(string userId, RoleChangedEvent eventData, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group(GetPersonalChannelName(userId))
            .SendAsync("RoleChanged", eventData, cancellationToken);
    }

    #endregion

    #region Presence Events

    public async Task SendUserOnlineAsync(IEnumerable<string> userIds, UserOnlineEvent eventData, CancellationToken cancellationToken = default)
    {
        List<string> personalChannels = userIds.Select(GetPersonalChannelName).ToList();

        if (personalChannels.Count == 0)
        {
            return;
        }

        _logger.LogDebug("Broadcasting UserOnline for {UserId} to {Count} users", eventData.UserId, personalChannels.Count);

        await _hubContext.Clients
            .Groups(personalChannels)
            .SendAsync("UserOnline", eventData, cancellationToken);
    }

    public async Task SendUserOfflineAsync(IEnumerable<string> userIds, UserOfflineEvent eventData, CancellationToken cancellationToken = default)
    {
        List<string> personalChannels = userIds.Select(GetPersonalChannelName).ToList();

        if (personalChannels.Count == 0)
        {
            return;
        }

        _logger.LogDebug("Broadcasting UserOffline for {UserId} to {Count} users", eventData.UserId, personalChannels.Count);

        await _hubContext.Clients
            .Groups(personalChannels)
            .SendAsync("UserOffline", eventData, cancellationToken);
    }

    #endregion

    private static string GetGroupName(Guid groupId)
    {
        return $"group-{groupId}";
    }

    private static string GetPersonalChannelName(string userId)
    {
        return $"user-{userId}";
    }
}
