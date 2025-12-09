namespace AiGroupChat.Application.DTOs.SignalR.PersonalChannel;

/// <summary>
/// Sent when user is added to a new group.
/// </summary>
public class AddedToGroupEvent
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string AddedByName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
}