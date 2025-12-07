using AiGroupChat.Application.DTOs.Groups;

namespace AiGroupChat.Application.Interfaces;

public interface IGroupMemberService
{
    /// <summary>
    /// Add a member to a group (Owner or Admin only)
    /// </summary>
    Task<GroupMemberResponse> AddMemberAsync(Guid groupId, AddMemberRequest request, string currentUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all members of a group (any member can view)
    /// </summary>
    Task<List<GroupMemberResponse>> GetMembersAsync(Guid groupId, string currentUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a member's role (Owner only, Admin <-> Member)
    /// </summary>
    Task<GroupMemberResponse> UpdateMemberRoleAsync(Guid groupId, string userId, UpdateMemberRoleRequest request, string currentUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a member from a group (Owner can remove anyone, Admin can remove Members only)
    /// </summary>
    Task RemoveMemberAsync(Guid groupId, string userId, string currentUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Leave a group (any member except Owner)
    /// </summary>
    Task LeaveGroupAsync(Guid groupId, string currentUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Transfer ownership to another member (Owner only)
    /// </summary>
    Task<GroupMemberResponse> TransferOwnershipAsync(Guid groupId, TransferOwnershipRequest request, string currentUserId, CancellationToken cancellationToken = default);
}