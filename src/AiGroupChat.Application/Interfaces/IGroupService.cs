using AiGroupChat.Application.DTOs.Groups;

namespace AiGroupChat.Application.Interfaces;

public interface IGroupService
{
    /// <summary>
    /// Create a new group (creator becomes admin)
    /// </summary>
    Task<GroupResponse> CreateAsync(CreateGroupRequest request, string currentUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all groups where the current user is a member
    /// </summary>
    Task<List<GroupResponse>> GetMyGroupsAsync(string currentUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get group details by ID (must be a member)
    /// </summary>
    Task<GroupResponse> GetByIdAsync(Guid groupId, string currentUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a group (must be an admin)
    /// </summary>
    Task<GroupResponse> UpdateAsync(Guid groupId, UpdateGroupRequest request, string currentUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a group (must be an admin)
    /// </summary>
    Task DeleteAsync(Guid groupId, string currentUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update AI settings for a group (must be an admin)
    /// </summary>
    Task<GroupResponse> UpdateAiSettingsAsync(Guid groupId, UpdateAiSettingsRequest request, string currentUserId, CancellationToken cancellationToken = default);
}