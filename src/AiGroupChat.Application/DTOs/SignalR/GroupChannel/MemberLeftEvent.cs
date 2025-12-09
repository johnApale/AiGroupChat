namespace AiGroupChat.Application.DTOs.SignalR.GroupChannel;

/// <summary>
/// Sent when a member leaves or is removed from the group.
/// Broadcast to group channel for active viewers.
/// </summary>
public class MemberLeftEvent
{
    public Guid GroupId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime LeftAt { get; set; }
}