namespace AiGroupChat.Application.DTOs.SignalR.PersonalChannel;

/// <summary>
/// Sent when a user goes offline (last connection closed).
/// </summary>
public class UserOfflineEvent
{
    public string UserId { get; set; } = string.Empty;
    public DateTime OfflineAt { get; set; }
}