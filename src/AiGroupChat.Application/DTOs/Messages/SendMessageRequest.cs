using System.ComponentModel.DataAnnotations;

namespace AiGroupChat.Application.DTOs.Messages;

/// <summary>
/// Request to send a message to a group.
/// </summary>
public class SendMessageRequest
{
    /// <summary>
    /// Message content. Use @provider (e.g., @gemini) to invoke AI.
    /// </summary>
    /// <example>@gemini What do you think about this approach?</example>
    [Required]
    [MaxLength(10000)]
    public string Content { get; set; } = string.Empty;
}