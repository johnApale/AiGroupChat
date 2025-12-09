namespace AiGroupChat.Application.DTOs.SignalR.GroupChannel;

public class UserTypingEvent
{
    public Guid GroupId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
