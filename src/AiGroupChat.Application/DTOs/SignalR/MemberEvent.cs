using AiGroupChat.Application.DTOs.Groups;

namespace AiGroupChat.Application.DTOs.SignalR;

public class MemberAddedEvent
{
    public Guid GroupId { get; set; }
    public GroupMemberResponse Member { get; set; } = null!;
}

public class MemberRemovedEvent
{
    public Guid GroupId { get; set; }
    public string UserId { get; set; } = string.Empty;
}

public class MemberRoleChangedEvent
{
    public Guid GroupId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string NewRole { get; set; } = string.Empty;
}
