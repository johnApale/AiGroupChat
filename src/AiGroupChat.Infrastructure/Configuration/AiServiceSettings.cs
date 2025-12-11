namespace AiGroupChat.Infrastructure.Configuration;

public class AiServiceSettings
{
    public const string SectionName = "AiService";

    /// <summary>
    /// Base URL of the Python AI service (e.g., http://localhost:8000)
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// API key for authenticating with the AI service.
    /// Should be set via environment variable in production.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum number of messages to include as context when calling AI
    /// </summary>
    public int MaxContextMessages { get; set; } = 100;
}