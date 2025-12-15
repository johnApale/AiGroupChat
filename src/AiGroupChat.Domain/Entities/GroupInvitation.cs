using AiGroupChat.Domain.Enums;

namespace AiGroupChat.Domain.Entities;

public class GroupInvitation
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string InvitedById { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    
    // Tracking timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime LastSentAt { get; set; }
    public int SendCount { get; set; } = 1;
    
    // Acceptance tracking
    public DateTime? AcceptedAt { get; set; }
    public string? AcceptedByUserId { get; set; }
    
    // Revocation tracking
    public DateTime? RevokedAt { get; set; }
    public string? RevokedById { get; set; }

    // Navigation properties
    public Group Group { get; set; } = null!;
    public User InvitedBy { get; set; } = null!;
    public User? AcceptedByUser { get; set; }
    public User? RevokedBy { get; set; }
}