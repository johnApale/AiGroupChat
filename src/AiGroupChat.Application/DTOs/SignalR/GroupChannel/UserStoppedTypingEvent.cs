namespace AiGroupChat.Application.DTOs.SignalR.GroupChannel;

/// <summary>
/// Sent when a user stops typing in a group.
/// Broadcast to group channel for active viewers.
/// </summary>
public class UserStoppedTypingEvent
{
    public Guid GroupId { get; set; }
    public string UserId { get; set; } = string.Empty;
}