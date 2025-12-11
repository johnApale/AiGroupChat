namespace AiGroupChat.Application.DTOs.Messages;

/// <summary>
/// Message details.
/// </summary>
public class MessageResponse
{
    /// <summary>
    /// Unique message identifier.
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Group this message belongs to.
    /// </summary>
    /// <example>660e8400-e29b-41d4-a716-446655440000</example>
    public Guid GroupId { get; set; }

    /// <summary>
    /// User ID of the sender. Null for AI messages.
    /// </summary>
    /// <example>770e8400-e29b-41d4-a716-446655440000</example>
    public string? SenderId { get; set; }

    /// <summary>
    /// Username of the sender. Null for AI messages.
    /// </summary>
    /// <example>johndoe</example>
    public string? SenderUserName { get; set; }

    /// <summary>
    /// Display name of the sender. For AI messages, shows provider name.
    /// </summary>
    /// <example>John Doe</example>
    public string? SenderDisplayName { get; set; }

    /// <summary>
    /// Type of sender. Values: "user", "ai".
    /// </summary>
    /// <example>user</example>
    public string SenderType { get; set; } = string.Empty;

    /// <summary>
    /// Message content.
    /// </summary>
    /// <example>@gemini What do you think about this approach?</example>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// URL to attachment file, if any.
    /// </summary>
    /// <example>https://storage.example.com/attachments/diagram.png</example>
    public string? AttachmentUrl { get; set; }

    /// <summary>
    /// MIME type of attachment. E.g., "image/png", "application/pdf".
    /// </summary>
    /// <example>image/png</example>
    public string? AttachmentType { get; set; }

    /// <summary>
    /// Original filename of attachment.
    /// </summary>
    /// <example>architecture-diagram.png</example>
    public string? AttachmentName { get; set; }

    /// <summary>
    /// When the message was sent.
    /// </summary>
    /// <example>2025-01-15T10:30:00Z</example>
    public DateTime CreatedAt { get; set; }
}