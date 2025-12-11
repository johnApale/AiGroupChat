using System.ComponentModel.DataAnnotations;

namespace AiGroupChat.Application.DTOs.Groups;

/// <summary>
/// Request to create a new group.
/// </summary>
public class CreateGroupRequest
{
    /// <summary>
    /// Name of the group. Displayed in group lists and headers.
    /// </summary>
    /// <example>Project Alpha Team</example>
    [Required(ErrorMessage = "Group name is required")]
    [MaxLength(200, ErrorMessage = "Group name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;
}