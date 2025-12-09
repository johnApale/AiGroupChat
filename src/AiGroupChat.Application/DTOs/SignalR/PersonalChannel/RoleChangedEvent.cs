namespace AiGroupChat.Application.DTOs.SignalR.PersonalChannel;

/// <summary>
/// Sent when user's role in a group changes.
/// </summary>
public class RoleChangedEvent
{
    public Guid GroupId { get; set; }
    public DateTime ChangedAt { get; set; }
}