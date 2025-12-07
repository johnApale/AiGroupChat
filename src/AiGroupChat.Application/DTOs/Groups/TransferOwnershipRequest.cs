using System.ComponentModel.DataAnnotations;

namespace AiGroupChat.Application.DTOs.Groups;

public class TransferOwnershipRequest
{
    [Required]
    public string NewOwnerUserId { get; set; } = string.Empty;
}