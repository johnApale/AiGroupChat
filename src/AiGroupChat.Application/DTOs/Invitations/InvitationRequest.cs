namespace AiGroupChat.Application.DTOs.Invitations;

/// <summary>
/// Response representing a group invitation.
/// </summary>
public class InvitationResponse
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string InvitedByUserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime LastSentAt { get; set; }
    public int SendCount { get; set; }
}