namespace AiGroupChat.Application.DTOs.Messages;

public class MessageResponse
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public string? SenderId { get; set; }
    public string? SenderUserName { get; set; }
    public string? SenderDisplayName { get; set; }
    public string SenderType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public string? AttachmentType { get; set; }
    public string? AttachmentName { get; set; }
    public DateTime CreatedAt { get; set; }
}
