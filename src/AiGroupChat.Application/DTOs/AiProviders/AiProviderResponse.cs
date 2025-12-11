namespace AiGroupChat.Application.DTOs.AiProviders;

/// <summary>
/// AI provider details.
/// </summary>
public class AiProviderResponse
{
    /// <summary>
    /// Unique provider identifier. Use when configuring group AI settings.
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Provider identifier used for @mentions (e.g., "gemini", "claude").
    /// </summary>
    /// <example>gemini</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable provider name for display.
    /// </summary>
    /// <example>Google Gemini</example>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Default model used for this provider.
    /// </summary>
    /// <example>gemini-1.5-pro</example>
    public string DefaultModel { get; set; } = string.Empty;

    /// <summary>
    /// Default temperature setting (0.0 - 1.0). Higher = more creative.
    /// </summary>
    /// <example>0.7</example>
    public decimal DefaultTemperature { get; set; }

    /// <summary>
    /// Maximum tokens the provider supports in a single request.
    /// </summary>
    /// <example>128000</example>
    public int MaxTokensLimit { get; set; }
}