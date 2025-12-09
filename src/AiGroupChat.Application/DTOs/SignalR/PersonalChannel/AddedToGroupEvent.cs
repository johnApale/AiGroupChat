namespace AiGroupChat.Application.DTOs.SignalR.PersonalChannel;

/// <summary>
/// Sent when user is added to a new group.
/// </summary>
public class AddedToGroupEvent
{
    public Guid GroupId { get; set; }
    public DateTime AddedAt { get; set; }
}