namespace AiGroupChat.Application.DTOs.AiService;

/// <summary>
/// Request to the Python AI service to generate a response.
/// This is an internal DTO used for service-to-service communication.
/// </summary>
public class AiGenerateRequest
{
    /// <summary>
    /// Provider identifier (e.g., "gemini", "claude", "openai", "grok").
    /// </summary>
    /// <example>gemini</example>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Conversation context - messages visible to the AI.
    /// </summary>
    public List<AiContextMessage> Context { get; set; } = new();

    /// <summary>
    /// The user's query/message that triggered the AI invocation.
    /// </summary>
    /// <example>What do you think about using React for this project?</example>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Configuration options for the AI generation.
    /// </summary>
    public AiGenerateConfig Config { get; set; } = new();
}

/// <summary>
/// A message in the conversation context sent to the AI.
/// </summary>
public class AiContextMessage
{
    /// <summary>
    /// Unique message identifier.
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Type of sender. Values: "user", "ai".
    /// </summary>
    /// <example>user</example>
    public string SenderType { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the sender.
    /// </summary>
    /// <example>John Doe</example>
    public string SenderName { get; set; } = string.Empty;

    /// <summary>
    /// Message content.
    /// </summary>
    /// <example>I think we should use React for the frontend.</example>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// When the message was sent.
    /// </summary>
    /// <example>2025-01-15T10:30:00Z</example>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Configuration options for AI generation.
/// </summary>
public class AiGenerateConfig
{
    /// <summary>
    /// Temperature setting (0.0 - 1.0). Higher values = more creative responses.
    /// </summary>
    /// <example>0.7</example>
    public decimal Temperature { get; set; } = 0.7m;

    /// <summary>
    /// Maximum tokens in the AI response.
    /// </summary>
    /// <example>2000</example>
    public int MaxTokens { get; set; } = 2000;
}