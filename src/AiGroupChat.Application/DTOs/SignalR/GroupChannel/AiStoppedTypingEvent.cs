namespace AiGroupChat.Application.DTOs.SignalR.GroupChannel;

/// <summary>
/// Sent when AI finishes generating a response (success or failure).
/// Broadcast to group channel for active viewers.
/// </summary>
public class AiStoppedTypingEvent
{
    public Guid GroupId { get; set; }
    public Guid ProviderId { get; set; }
}