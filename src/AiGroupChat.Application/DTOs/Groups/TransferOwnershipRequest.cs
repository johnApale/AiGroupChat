using System.ComponentModel.DataAnnotations;

namespace AiGroupChat.Application.DTOs.Groups;

/// <summary>
/// Request to transfer group ownership.
/// </summary>
public class TransferOwnershipRequest
{
    /// <summary>
    /// User ID of the member who will become the new owner.
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    [Required]
    public string NewOwnerUserId { get; set; } = string.Empty;
}