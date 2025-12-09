using System.Security.Claims;
using AiGroupChat.Application.DTOs.SignalR.GroupChannel;
using AiGroupChat.Application.DTOs.SignalR.PersonalChannel;
using AiGroupChat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AiGroupChat.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IGroupRepository _groupRepository;
    private readonly IUserRepository _userRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly IConnectionTracker _connectionTracker;
    private readonly IChatHubService _chatHubService;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(
        IGroupRepository groupRepository,
        IUserRepository userRepository,
        IGroupMemberRepository groupMemberRepository,
        IConnectionTracker connectionTracker,
        IChatHubService chatHubService,
        ILogger<ChatHub> logger)
    {
        _groupRepository = groupRepository;
        _userRepository = userRepository;
        _groupMemberRepository = groupMemberRepository;
        _connectionTracker = connectionTracker;
        _chatHubService = chatHubService;
        _logger = logger;
    }

    /// <summary>
    /// Subscribe to a group's real-time updates
    /// </summary>
    public async Task JoinGroup(Guid groupId)
    {
        string userId = GetUserId();

        // Verify user is a member of the group
        bool isMember = await _groupRepository.IsMemberAsync(groupId, userId);

        if (!isMember)
        {
            throw new HubException("You are not a member of this group.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(groupId));
    }

    /// <summary>
    /// Unsubscribe from a group's real-time updates
    /// </summary>
    public async Task LeaveGroup(Guid groupId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(groupId));
    }

    /// <summary>
    /// Notify group members that user started typing
    /// </summary>
    public async Task StartTyping(Guid groupId)
    {
        string userId = GetUserId();

        // Verify user is a member of the group
        bool isMember = await _groupRepository.IsMemberAsync(groupId, userId);

        if (!isMember)
        {
            return; // Silently ignore if not a member
        }

        Domain.Entities.User? user = await _userRepository.FindByIdAsync(userId);

        if (user == null)
        {
            return;
        }

        UserTypingEvent typingEvent = new UserTypingEvent
        {
            GroupId = groupId,
            UserId = userId,
            UserName = user.UserName ?? string.Empty,
            DisplayName = user.DisplayName
        };

        await Clients.OthersInGroup(GetGroupName(groupId)).SendAsync("UserTyping", typingEvent);
    }

    /// <summary>
    /// Notify group members that user stopped typing
    /// </summary>
    public async Task StopTyping(Guid groupId)
    {
        string userId = GetUserId();

        // Verify user is a member of the group
        bool isMember = await _groupRepository.IsMemberAsync(groupId, userId);

        if (!isMember)
        {
            return; // Silently ignore if not a member
        }

        UserStoppedTypingEvent stoppedTypingEvent = new UserStoppedTypingEvent
        {
            GroupId = groupId,
            UserId = userId
        };

        await Clients.OthersInGroup(GetGroupName(groupId)).SendAsync("UserStoppedTyping", stoppedTypingEvent);
    }

    public override async Task OnConnectedAsync()
    {
        string userId = GetUserId();
        string connectionId = Context.ConnectionId;

        // Auto-join personal channel
        await Groups.AddToGroupAsync(connectionId, GetPersonalChannelName(userId));

        // Track connection and broadcast presence if first connection
        bool isFirstConnection = _connectionTracker.AddConnection(userId, connectionId);

        if (isFirstConnection)
        {
            _logger.LogInformation("User {UserId} came online (first connection)", userId);

            // Get user info for the event
            Domain.Entities.User? user = await _userRepository.FindByIdAsync(userId);

            // Get all users who share groups with this user
            List<string> sharedUserIds = await _groupMemberRepository.GetUsersWhoShareGroupsWithAsync(userId);

            if (sharedUserIds.Count > 0)
            {
                UserOnlineEvent onlineEvent = new UserOnlineEvent
                {
                    UserId = userId,
                    DisplayName = user?.DisplayName ?? user?.UserName ?? string.Empty,
                    OnlineAt = DateTime.UtcNow
                };

                await _chatHubService.SendUserOnlineAsync(sharedUserIds, onlineEvent);
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string userId = GetUserId();
        string connectionId = Context.ConnectionId;

        // Remove connection and broadcast presence if last connection
        bool wasLastConnection = _connectionTracker.RemoveConnection(userId, connectionId);

        if (wasLastConnection)
        {
            _logger.LogInformation("User {UserId} went offline (last connection closed)", userId);

            // Get user info for the event
            Domain.Entities.User? user = await _userRepository.FindByIdAsync(userId);

            // Get all users who share groups with this user
            List<string> sharedUserIds = await _groupMemberRepository.GetUsersWhoShareGroupsWithAsync(userId);

            if (sharedUserIds.Count > 0)
            {
                UserOfflineEvent offlineEvent = new UserOfflineEvent
                {
                    UserId = userId,
                    DisplayName = user?.DisplayName ?? user?.UserName ?? string.Empty,
                    OfflineAt = DateTime.UtcNow
                };

                await _chatHubService.SendUserOfflineAsync(sharedUserIds, offlineEvent);
            }
        }

        // SignalR auto-removes from all groups on disconnect
        await base.OnDisconnectedAsync(exception);
    }

    private string GetUserId()
    {
        string? userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            throw new HubException("User not authenticated.");
        }

        return userId;
    }

    private static string GetGroupName(Guid groupId)
    {
        return $"group-{groupId}";
    }

    private static string GetPersonalChannelName(string userId)
    {
        return $"user-{userId}";
    }
}