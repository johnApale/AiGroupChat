namespace AiGroupChat.Application.DTOs.SignalR.GroupChannel;

/// <summary>
/// Sent when AI monitoring is toggled or AI provider changes.
/// Broadcast to group channel for active viewers.
/// </summary>
public class AiSettingsChangedEvent
{
    public Guid GroupId { get; set; }
    public bool AiMonitoringEnabled { get; set; }
    public Guid? AiProviderId { get; set; }
    public string? AiProviderName { get; set; }
    public string ChangedByName { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
}