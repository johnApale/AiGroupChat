using System.ComponentModel.DataAnnotations;

namespace AiGroupChat.Application.DTOs.Invitations;

/// <summary>
/// Request to invite members to a group via email.
/// </summary>
public class InviteMembersRequest
{
    /// <summary>
    /// List of email addresses to invite.
    /// </summary>
    /// <example>["user1@example.com", "user2@example.com"]</example>
    [Required]
    [MinLength(1, ErrorMessage = "At least one email address is required.")]
    public List<string> Emails { get; set; } = new();
}