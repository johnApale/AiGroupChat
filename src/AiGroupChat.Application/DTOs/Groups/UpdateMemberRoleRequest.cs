using System.ComponentModel.DataAnnotations;

namespace AiGroupChat.Application.DTOs.Groups;

/// <summary>
/// Request to update a member's role.
/// </summary>
public class UpdateMemberRoleRequest
{
    /// <summary>
    /// Role to assign. Valid values: "Admin", "Member".
    /// </summary>
    /// <example>Admin</example>
    [Required]
    public string Role { get; set; } = string.Empty;
}