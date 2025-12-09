namespace AiGroupChat.Application.DTOs.Groups;

public class UpdateAiSettingsRequest
{
    public bool? AiMonitoringEnabled { get; set; }
    public Guid? AiProviderId { get; set; }
}
