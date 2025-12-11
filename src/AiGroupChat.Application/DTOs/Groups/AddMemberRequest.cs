using System.ComponentModel.DataAnnotations;

namespace AiGroupChat.Application.DTOs.Groups;

/// <summary>
/// Request to add a member to a group.
/// </summary>
public class AddMemberRequest
{
    /// <summary>
    /// User ID of the person to add.
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    [Required]
    public string UserId { get; set; } = string.Empty;
}