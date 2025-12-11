namespace AiGroupChat.Application.DTOs.SignalR.GroupChannel;

/// <summary>
/// Sent when AI starts generating a response.
/// Broadcast to group channel for active viewers.
/// </summary>
public class AiTypingEvent
{
    public Guid GroupId { get; set; }
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
}