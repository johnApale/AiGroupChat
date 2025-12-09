namespace AiGroupChat.Application.DTOs.SignalR.GroupChannel;

/// <summary>
/// Sent when a member's role changes in the group.
/// Broadcast to group channel for active viewers.
/// </summary>
public class MemberRoleChangedEvent
{
    public Guid GroupId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string OldRole { get; set; } = string.Empty;
    public string NewRole { get; set; } = string.Empty;
}