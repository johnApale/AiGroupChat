using System.ComponentModel.DataAnnotations;

namespace AiGroupChat.Application.DTOs.Groups;

public class UpdateMemberRoleRequest
{
    /// <summary>
    /// Role to assign: "Admin" or "Member"
    /// </summary>
    [Required]
    public string Role { get; set; } = string.Empty;
}