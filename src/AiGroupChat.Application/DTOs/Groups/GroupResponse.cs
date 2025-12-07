namespace AiGroupChat.Application.DTOs.Groups;

public class GroupResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CreatedById { get; set; } = string.Empty;
    public bool AiMonitoringEnabled { get; set; }
    public Guid? AiProviderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<GroupMemberResponse> Members { get; set; } = new();
}