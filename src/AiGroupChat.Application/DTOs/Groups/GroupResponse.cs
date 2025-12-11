using AiGroupChat.Application.DTOs.AiProviders;

namespace AiGroupChat.Application.DTOs.Groups;

/// <summary>
/// Group details with members.
/// </summary>
public class GroupResponse
{
    /// <summary>
    /// Unique group identifier.
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Group name.
    /// </summary>
    /// <example>Project Alpha Team</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// User ID of the group creator/owner.
    /// </summary>
    /// <example>660e8400-e29b-41d4-a716-446655440000</example>
    public string CreatedById { get; set; } = string.Empty;

    /// <summary>
    /// Whether AI monitoring is enabled. When true, messages are visible to AI.
    /// </summary>
    /// <example>true</example>
    public bool AiMonitoringEnabled { get; set; }

    /// <summary>
    /// Currently configured AI provider ID.
    /// </summary>
    /// <example>770e8400-e29b-41d4-a716-446655440000</example>
    public Guid AiProviderId { get; set; }

    /// <summary>
    /// AI provider details.
    /// </summary>
    public AiProviderResponse AiProvider { get; set; } = null!;

    /// <summary>
    /// When the group was created.
    /// </summary>
    /// <example>2025-01-15T10:30:00Z</example>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the group was last updated.
    /// </summary>
    /// <example>2025-01-15T14:20:00Z</example>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// List of group members with their roles.
    /// </summary>
    public List<GroupMemberResponse> Members { get; set; } = new();
}