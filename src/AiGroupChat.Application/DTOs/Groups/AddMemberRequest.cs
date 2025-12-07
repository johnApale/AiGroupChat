using System.ComponentModel.DataAnnotations;

namespace AiGroupChat.Application.DTOs.Groups;

public class AddMemberRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;
}