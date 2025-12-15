namespace AiGroupChat.Domain.Entities;

public class Group
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CreatedById { get; set; } = string.Empty;
    public bool AiMonitoringEnabled { get; set; } = false;
    public Guid AiProviderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public User CreatedBy { get; set; } = null!;
    public AiProvider AiProvider { get; set; } = null!;
    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<GroupInvitation> Invitations { get; set; } = new List<GroupInvitation>();
}