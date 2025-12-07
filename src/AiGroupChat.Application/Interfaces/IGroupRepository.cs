using AiGroupChat.Domain.Entities;

namespace AiGroupChat.Application.Interfaces;

public interface IGroupRepository
{
    /// <summary>
    /// Create a new group
    /// </summary>
    Task<Group> CreateAsync(Group group, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a group by ID with members included
    /// </summary>
    Task<Group?> GetByIdWithMembersAsync(Guid groupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all groups where the user is a member, with members included
    /// </summary>
    Task<List<Group>> GetGroupsByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a group
    /// </summary>
    Task<Group> UpdateAsync(Group group, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a group
    /// </summary>
    Task DeleteAsync(Group group, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a user is a member of a group
    /// </summary>
    Task<bool> IsMemberAsync(Guid groupId, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a user is an admin or owner of a group
    /// </summary>
    Task<bool> IsAdminAsync(Guid groupId, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a user is the owner of a group
    /// </summary>
    Task<bool> IsOwnerAsync(Guid groupId, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a member to a group
    /// </summary>
    Task<GroupMember> AddMemberAsync(GroupMember member, CancellationToken cancellationToken = default);
}