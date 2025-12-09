namespace AiGroupChat.Application.DTOs.SignalR.PersonalChannel;

/// <summary>
/// Sent when a user comes online (first connection established).
/// </summary>
public class UserOnlineEvent
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime OnlineAt { get; set; }
}