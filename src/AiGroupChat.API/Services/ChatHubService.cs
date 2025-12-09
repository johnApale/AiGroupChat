using AiGroupChat.API.Hubs;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.DTOs.Messages;
using AiGroupChat.Application.DTOs.SignalR;
using AiGroupChat.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AiGroupChat.API.Services;

public class ChatHubService : IChatHubService
{
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatHubService(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
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

    private static string GetGroupName(Guid groupId)
    {
        return $"group-{groupId}";
    }
}
