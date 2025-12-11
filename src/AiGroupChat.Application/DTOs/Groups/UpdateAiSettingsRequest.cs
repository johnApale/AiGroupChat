namespace AiGroupChat.Application.DTOs.Groups;

/// <summary>
/// Request to update group AI settings.
/// </summary>
public class UpdateAiSettingsRequest
{
    /// <summary>
    /// Enable or disable AI monitoring. When enabled, new messages are visible to AI.
    /// </summary>
    /// <example>true</example>
    public bool? AiMonitoringEnabled { get; set; }

    /// <summary>
    /// AI provider ID. Get available providers from /api/ai-providers.
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    public Guid? AiProviderId { get; set; }
}