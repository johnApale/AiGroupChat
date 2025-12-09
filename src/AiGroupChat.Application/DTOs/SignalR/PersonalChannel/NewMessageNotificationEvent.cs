namespace AiGroupChat.Application.DTOs.SignalR.PersonalChannel;

/// <summary>
/// Sent when a new message is posted in any group the user belongs to.
/// Used for notification badge/drawer.
/// </summary>
public class NewMessageNotificationEvent
{
    public Guid GroupId { get; set; }
    public DateTime SentAt { get; set; }
}