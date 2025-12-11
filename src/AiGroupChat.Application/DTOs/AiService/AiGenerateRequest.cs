namespace AiGroupChat.Application.DTOs.AiService;

/// <summary>
/// Request to the Python AI service to generate a response
/// </summary>
public class AiGenerateRequest
{
    public string Provider { get; set; } = string.Empty;
    public List<AiContextMessage> Context { get; set; } = new();
    public string Query { get; set; } = string.Empty;
    public AiGenerateConfig Config { get; set; } = new();
}

/// <summary>
/// A message in the conversation context sent to the AI
/// </summary>
public class AiContextMessage
{
    public string Id { get; set; } = string.Empty;
    public string SenderType { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Configuration options for AI generation
/// </summary>
public class AiGenerateConfig
{
    public decimal Temperature { get; set; } = 0.7m;
    public int MaxTokens { get; set; } = 2000;
}