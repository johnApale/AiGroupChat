namespace AiGroupChat.Application.DTOs.SignalR.GroupChannel;

/// <summary>
/// Sent when a new member joins the group.
/// Broadcast to group channel for active viewers.
/// </summary>
public class MemberJoinedEvent
{
    public Guid GroupId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}