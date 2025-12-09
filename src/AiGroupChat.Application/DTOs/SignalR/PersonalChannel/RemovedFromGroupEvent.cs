namespace AiGroupChat.Application.DTOs.SignalR.PersonalChannel;

/// <summary>
/// Sent when user is removed from a group.
/// </summary>
public class RemovedFromGroupEvent
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public DateTime RemovedAt { get; set; }
}