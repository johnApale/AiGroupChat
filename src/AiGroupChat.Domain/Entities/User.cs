using Microsoft.AspNetCore.Identity;

namespace AiGroupChat.Domain.Entities;

public class User : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Group> CreatedGroups { get; set; } = new List<Group>();
    public ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<GroupInvitation> SentInvitations { get; set; } = new List<GroupInvitation>();
    public ICollection<GroupInvitation> AcceptedInvitations { get; set; } = new List<GroupInvitation>();
    public ICollection<GroupInvitation> RevokedInvitations { get; set; } = new List<GroupInvitation>();
}