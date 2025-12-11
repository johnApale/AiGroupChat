namespace AiGroupChat.Application.DTOs.Groups;

/// <summary>
/// Group member details.
/// </summary>
public class GroupMemberResponse
{
    /// <summary>
    /// User ID of the member.
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Member's username.
    /// </summary>
    /// <example>johndoe</example>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Member's display name.
    /// </summary>
    /// <example>John Doe</example>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Member's role in the group. Values: "Owner", "Admin", "Member".
    /// </summary>
    /// <example>Admin</example>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// When the member joined the group.
    /// </summary>
    /// <example>2025-01-15T10:30:00Z</example>
    public DateTime JoinedAt { get; set; }
}