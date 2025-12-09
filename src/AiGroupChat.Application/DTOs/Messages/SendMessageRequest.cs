using System.ComponentModel.DataAnnotations;

namespace AiGroupChat.Application.DTOs.Messages;

public class SendMessageRequest
{
    [Required]
    [MaxLength(10000)]
    public string Content { get; set; } = string.Empty;
}
