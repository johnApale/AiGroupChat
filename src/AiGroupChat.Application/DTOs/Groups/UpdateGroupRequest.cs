using System.ComponentModel.DataAnnotations;

namespace AiGroupChat.Application.DTOs.Groups;

/// <summary>
/// Request to update group details.
/// </summary>
public class UpdateGroupRequest
{
    /// <summary>
    /// New name for the group.
    /// </summary>
    /// <example>Project Alpha Team - Phase 2</example>
    [Required(ErrorMessage = "Group name is required")]
    [MaxLength(200, ErrorMessage = "Group name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;
}