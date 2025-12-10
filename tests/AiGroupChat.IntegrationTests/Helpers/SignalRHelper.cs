using AiGroupChat.Application.DTOs.Messages;
using AiGroupChat.Application.DTOs.SignalR.GroupChannel;
using AiGroupChat.Application.DTOs.SignalR.PersonalChannel;
using Microsoft.AspNetCore.SignalR.Client;

namespace AiGroupChat.IntegrationTests.Helpers;

/// <summary>
/// Helper class for managing a single SignalR connection in integration tests.
/// Provides event collection, hub method invocation, and waiting utilities.
/// </summary>
public class SignalRHelper : IAsyncDisposable
{
    private readonly string _hubUrl;
    private readonly HttpMessageHandler _httpMessageHandler;
    private HubConnection? _connection;
    private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(5);

    #region Event Collectors

    // Group Channel Events
    public List<MessageResponse> ReceivedMessages { get; } = new();
    public List<UserTypingEvent> TypingEvents { get; } = new();
    public List<UserStoppedTypingEvent> StoppedTypingEvents { get; } = new();
    public List<MemberJoinedEvent> MemberJoinedEvents { get; } = new();
    public List<MemberLeftEvent> MemberLeftEvents { get; } = new();
    public List<MemberRoleChangedEvent> MemberRoleChangedEvents { get; } = new();
    public List<AiSettingsChangedEvent> AiSettingsChangedEvents { get; } = new();

    // Personal Channel Events
    public List<AddedToGroupEvent> AddedToGroupEvents { get; } = new();
    public List<RemovedFromGroupEvent> RemovedFromGroupEvents { get; } = new();
    public List<RoleChangedEvent> RoleChangedEvents { get; } = new();
    public List<GroupActivityEvent> GroupActivityEvents { get; } = new();
    public List<NewMessageNotificationEvent> NewMessageNotificationEvents { get; } = new();
    public List<UserOnlineEvent> UserOnlineEvents { get; } = new();
    public List<UserOfflineEvent> UserOfflineEvents { get; } = new();

    #endregion

    /// <summary>
    /// Creates a new SignalRHelper for integration testing.
    /// </summary>
    /// <param name="hubUrl">The hub URL (e.g., "http://localhost/hubs/chat")</param>
    /// <param name="httpMessageHandler">The test server's HttpMessageHandler for in-memory requests</param>
    public SignalRHelper(string hubUrl, HttpMessageHandler httpMessageHandler)
    {
        _hubUrl = hubUrl;
        _httpMessageHandler = httpMessageHandler;
    }

    /// <summary>
    /// Gets the current connection state
    /// </summary>
    public HubConnectionState? ConnectionState => _connection?.State;

    /// <summary>
    /// Returns true if the connection is established
    /// </summary>
    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    #region Connection Lifecycle

    /// <summary>
    /// Establishes a SignalR connection with the given JWT access token
    /// </summary>
    public async Task ConnectAsync(string accessToken)
    {
        string urlWithToken = $"{_hubUrl}?access_token={accessToken}";

        _connection = new HubConnectionBuilder()
            .WithUrl(urlWithToken, options =>
            {
                // Use the test server's handler for in-memory HTTP requests
                options.HttpMessageHandlerFactory = _ => _httpMessageHandler;
            })
            .Build();

        RegisterEventHandlers();

        await _connection.StartAsync();
    }

    /// <summary>
    /// Disconnects from the SignalR hub
    /// </summary>
    public async Task DisconnectAsync()
    {
        if (_connection != null)
        {
            await _connection.StopAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    #endregion

    #region Hub Method Invocations

    /// <summary>
    /// Join a group to receive its real-time events
    /// </summary>
    public async Task JoinGroupAsync(Guid groupId)
    {
        EnsureConnected();
        await _connection!.InvokeAsync("JoinGroup", groupId);
    }

    /// <summary>
    /// Leave a group to stop receiving its real-time events
    /// </summary>
    public async Task LeaveGroupAsync(Guid groupId)
    {
        EnsureConnected();
        await _connection!.InvokeAsync("LeaveGroup", groupId);
    }

    /// <summary>
    /// Notify group members that this user started typing
    /// </summary>
    public async Task StartTypingAsync(Guid groupId)
    {
        EnsureConnected();
        await _connection!.InvokeAsync("StartTyping", groupId);
    }

    /// <summary>
    /// Notify group members that this user stopped typing
    /// </summary>
    public async Task StopTypingAsync(Guid groupId)
    {
        EnsureConnected();
        await _connection!.InvokeAsync("StopTyping", groupId);
    }

    #endregion

    #region Wait Methods

    /// <summary>
    /// Waits for a message matching the predicate, or throws TimeoutException
    /// </summary>
    public Task<MessageResponse> WaitForMessageAsync(
        Func<MessageResponse, bool> predicate,
        TimeSpan? timeout = null)
    {
        return WaitForEventAsync(ReceivedMessages, predicate, timeout);
    }

    /// <summary>
    /// Waits for a typing event matching the predicate
    /// </summary>
    public Task<UserTypingEvent> WaitForTypingEventAsync(
        Func<UserTypingEvent, bool> predicate,
        TimeSpan? timeout = null)
    {
        return WaitForEventAsync(TypingEvents, predicate, timeout);
    }

    /// <summary>
    /// Waits for a stopped typing event matching the predicate
    /// </summary>
    public Task<UserStoppedTypingEvent> WaitForStoppedTypingEventAsync(
        Func<UserStoppedTypingEvent, bool> predicate,
        TimeSpan? timeout = null)
    {
        return WaitForEventAsync(StoppedTypingEvents, predicate, timeout);
    }

    /// <summary>
    /// Waits for a member joined event matching the predicate
    /// </summary>
    public Task<MemberJoinedEvent> WaitForMemberJoinedEventAsync(
        Func<MemberJoinedEvent, bool> predicate,
        TimeSpan? timeout = null)
    {
        return WaitForEventAsync(MemberJoinedEvents, predicate, timeout);
    }

    /// <summary>
    /// Waits for a member left event matching the predicate
    /// </summary>
    public Task<MemberLeftEvent> WaitForMemberLeftEventAsync(
        Func<MemberLeftEvent, bool> predicate,
        TimeSpan? timeout = null)
    {
        return WaitForEventAsync(MemberLeftEvents, predicate, timeout);
    }

    /// <summary>
    /// Waits for a member role changed event matching the predicate
    /// </summary>
    public Task<MemberRoleChangedEvent> WaitForMemberRoleChangedEventAsync(
        Func<MemberRoleChangedEvent, bool> predicate,
        TimeSpan? timeout = null)
    {
        return WaitForEventAsync(MemberRoleChangedEvents, predicate, timeout);
    }

    /// <summary>
    /// Waits for an AI settings changed event matching the predicate
    /// </summary>
    public Task<AiSettingsChangedEvent> WaitForAiSettingsChangedEventAsync(
        Func<AiSettingsChangedEvent, bool> predicate,
        TimeSpan? timeout = null)
    {
        return WaitForEventAsync(AiSettingsChangedEvents, predicate, timeout);
    }

    /// <summary>
    /// Waits for an added to group event matching the predicate
    /// </summary>
    public Task<AddedToGroupEvent> WaitForAddedToGroupEventAsync(
        Func<AddedToGroupEvent, bool> predicate,
        TimeSpan? timeout = null)
    {
        return WaitForEventAsync(AddedToGroupEvents, predicate, timeout);
    }

    /// <summary>
    /// Waits for a removed from group event matching the predicate
    /// </summary>
    public Task<RemovedFromGroupEvent> WaitForRemovedFromGroupEventAsync(
        Func<RemovedFromGroupEvent, bool> predicate,
        TimeSpan? timeout = null)
    {
        return WaitForEventAsync(RemovedFromGroupEvents, predicate, timeout);
    }

    /// <summary>
    /// Waits for a role changed event matching the predicate
    /// </summary>
    public Task<RoleChangedEvent> WaitForRoleChangedEventAsync(
        Func<RoleChangedEvent, bool> predicate,
        TimeSpan? timeout = null)
    {
        return WaitForEventAsync(RoleChangedEvents, predicate, timeout);
    }

    /// <summary>
    /// Waits for a user online event matching the predicate
    /// </summary>
    public Task<UserOnlineEvent> WaitForUserOnlineEventAsync(
        Func<UserOnlineEvent, bool> predicate,
        TimeSpan? timeout = null)
    {
        return WaitForEventAsync(UserOnlineEvents, predicate, timeout);
    }

    /// <summary>
    /// Waits for a user offline event matching the predicate
    /// </summary>
    public Task<UserOfflineEvent> WaitForUserOfflineEventAsync(
        Func<UserOfflineEvent, bool> predicate,
        TimeSpan? timeout = null)
    {
        return WaitForEventAsync(UserOfflineEvents, predicate, timeout);
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Clears all collected events
    /// </summary>
    public void ClearEvents()
    {
        ReceivedMessages.Clear();
        TypingEvents.Clear();
        StoppedTypingEvents.Clear();
        MemberJoinedEvents.Clear();
        MemberLeftEvents.Clear();
        MemberRoleChangedEvents.Clear();
        AiSettingsChangedEvents.Clear();
        AddedToGroupEvents.Clear();
        RemovedFromGroupEvents.Clear();
        RoleChangedEvents.Clear();
        GroupActivityEvents.Clear();
        NewMessageNotificationEvents.Clear();
        UserOnlineEvents.Clear();
        UserOfflineEvents.Clear();
    }

    #endregion

    #region Private Methods

    private void RegisterEventHandlers()
    {
        if (_connection == null) return;

        // Group Channel Events
        _connection.On<MessageResponse>("MessageReceived", msg => ReceivedMessages.Add(msg));
        _connection.On<UserTypingEvent>("UserTyping", evt => TypingEvents.Add(evt));
        _connection.On<UserStoppedTypingEvent>("UserStoppedTyping", evt => StoppedTypingEvents.Add(evt));
        _connection.On<MemberJoinedEvent>("MemberJoined", evt => MemberJoinedEvents.Add(evt));
        _connection.On<MemberLeftEvent>("MemberLeft", evt => MemberLeftEvents.Add(evt));
        _connection.On<MemberRoleChangedEvent>("MemberRoleChanged", evt => MemberRoleChangedEvents.Add(evt));
        _connection.On<AiSettingsChangedEvent>("AiSettingsChanged", evt => AiSettingsChangedEvents.Add(evt));

        // Personal Channel Events
        _connection.On<AddedToGroupEvent>("AddedToGroup", evt => AddedToGroupEvents.Add(evt));
        _connection.On<RemovedFromGroupEvent>("RemovedFromGroup", evt => RemovedFromGroupEvents.Add(evt));
        _connection.On<RoleChangedEvent>("RoleChanged", evt => RoleChangedEvents.Add(evt));
        _connection.On<GroupActivityEvent>("GroupActivity", evt => GroupActivityEvents.Add(evt));
        _connection.On<NewMessageNotificationEvent>("NewMessageNotification", evt => NewMessageNotificationEvents.Add(evt));
        _connection.On<UserOnlineEvent>("UserOnline", evt => UserOnlineEvents.Add(evt));
        _connection.On<UserOfflineEvent>("UserOffline", evt => UserOfflineEvents.Add(evt));
    }

    private void EnsureConnected()
    {
        if (_connection == null || _connection.State != HubConnectionState.Connected)
        {
            throw new InvalidOperationException("SignalR connection is not established. Call ConnectAsync first.");
        }
    }

    private async Task<T> WaitForEventAsync<T>(
        List<T> eventList,
        Func<T, bool> predicate,
        TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = timeout ?? _defaultTimeout;
        DateTime deadline = DateTime.UtcNow + effectiveTimeout;

        while (DateTime.UtcNow < deadline)
        {
            // Check if event already exists
            T? existing = eventList.FirstOrDefault(predicate);
            if (existing != null)
            {
                return existing;
            }

            // Wait a bit before checking again
            await Task.Delay(50);
        }

        throw new TimeoutException(
            $"Timed out waiting for {typeof(T).Name} event after {effectiveTimeout.TotalSeconds} seconds. " +
            $"Events received: {eventList.Count}");
    }

    #endregion
}