namespace AiGroupChat.Application.DTOs.SignalR.PersonalChannel;

/// <summary>
/// Sent when any activity occurs in a group the user belongs to.
/// Used for home page list reordering.
/// </summary>
public class GroupActivityEvent
{
    public Guid GroupId { get; set; }
    public string? ActorName { get; set; }
}