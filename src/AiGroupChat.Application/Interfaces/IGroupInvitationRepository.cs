using AiGroupChat.Domain.Entities;

namespace AiGroupChat.Application.Interfaces;

public interface IGroupInvitationRepository
{
    /// <summary>
    /// Get invitation by ID.
    /// </summary>
    Task<GroupInvitation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get invitation by token (includes Group and InvitedBy).
    /// </summary>
    Task<GroupInvitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pending invitation for a specific email and group.
    /// </summary>
    Task<GroupInvitation?> GetPendingByEmailAndGroupAsync(string email, Guid groupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all pending invitations for a group.
    /// </summary>
    Task<List<GroupInvitation>> GetPendingByGroupAsync(Guid groupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new invitation.
    /// </summary>
    Task<GroupInvitation> CreateAsync(GroupInvitation invitation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing invitation.
    /// </summary>
    Task UpdateAsync(GroupInvitation invitation, CancellationToken cancellationToken = default);
}