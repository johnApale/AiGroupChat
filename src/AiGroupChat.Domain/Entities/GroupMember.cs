using AiGroupChat.Domain.Enums;

namespace AiGroupChat.Domain.Entities;

public class GroupMember
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public GroupRole Role { get; set; } = GroupRole.Member;
    public DateTime JoinedAt { get; set; }

    // Navigation properties
    public Group Group { get; set; } = null!;
    public User User { get; set; } = null!;
}