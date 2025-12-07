using System.ComponentModel.DataAnnotations;

namespace AiGroupChat.Application.DTOs.Groups;

public class CreateGroupRequest
{
    [Required(ErrorMessage = "Group name is required")]
    [MaxLength(200, ErrorMessage = "Group name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;
}