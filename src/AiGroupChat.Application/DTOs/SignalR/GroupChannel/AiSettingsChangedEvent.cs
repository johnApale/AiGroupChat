namespace AiGroupChat.Application.DTOs.SignalR.GroupChannel;

public class AiSettingsChangedEvent
{
    public Guid GroupId { get; set; }
    public bool AiMonitoringEnabled { get; set; }
    public Guid AiProviderId { get; set; }
    public string AiProviderName { get; set; } = string.Empty;
}
