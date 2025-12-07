using AiGroupChat.Domain.Enums;

namespace AiGroupChat.Domain.Entities;

public class Message
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public string? SenderId { get; set; }
    public SenderType SenderType { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool AiVisible { get; set; }
    public Guid? AiProviderId { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentType { get; set; }
    public string? AttachmentName { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Group Group { get; set; } = null!;
    public User? Sender { get; set; }
    public AiProvider? AiProvider { get; set; }
    public AiResponseMetadata? AiResponseMetadata { get; set; }
}