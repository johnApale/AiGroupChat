namespace AiGroupChat.Application.DTOs.SignalR.PersonalChannel;

/// <summary>
/// Sent when user's role in a group changes.
/// </summary>
public class RoleChangedEvent
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string OldRole { get; set; } = string.Empty;
    public string NewRole { get; set; } = string.Empty;
    public string ChangedByName { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
}