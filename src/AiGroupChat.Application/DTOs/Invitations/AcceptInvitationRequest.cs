using System.ComponentModel.DataAnnotations;

namespace AiGroupChat.Application.DTOs.Invitations;

/// <summary>
/// Request to accept a group invitation.
/// </summary>
public class AcceptInvitationRequest
{
    /// <summary>
    /// The invitation token from the email link.
    /// </summary>
    [Required]
    public string Token { get; set; } = string.Empty;
}