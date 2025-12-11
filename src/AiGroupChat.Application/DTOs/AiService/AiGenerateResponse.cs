namespace AiGroupChat.Application.DTOs.AiService;

/// <summary>
/// Response from the Python AI service
/// </summary>
public class AiGenerateResponse
{
    public string Response { get; set; } = string.Empty;
    public AiResponseMetadataDto Metadata { get; set; } = new();
    public AiAttachment? Attachment { get; set; }
}

/// <summary>
/// Metadata about the AI generation (tokens, latency, etc.)
/// </summary>
public class AiResponseMetadataDto
{
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int TokensInput { get; set; }
    public int TokensOutput { get; set; }
    public int LatencyMs { get; set; }
}

/// <summary>
/// Optional attachment returned by the AI (image, code file, etc.)
/// </summary>
public class AiAttachment
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Base64 { get; set; } = string.Empty;
}