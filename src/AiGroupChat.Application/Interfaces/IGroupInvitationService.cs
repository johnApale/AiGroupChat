using AiGroupChat.Application.DTOs.Invitations;

namespace AiGroupChat.Application.Interfaces;

public interface IGroupInvitationService
{
    /// <summary>
    /// Send invitations to multiple email addresses.
    /// </summary>
    Task<InviteMembersResponse> InviteMembersAsync(Guid groupId, InviteMembersRequest request, string currentUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all pending invitations for a group.
    /// </summary>
    Task<List<InvitationResponse>> GetPendingInvitationsAsync(Guid groupId, string currentUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoke a pending invitation.
    /// </summary>
    Task RevokeInvitationAsync(Guid groupId, Guid invitationId, string currentUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Accept an invitation using a token.
    /// </summary>
    Task<AcceptInvitationResponse> AcceptInvitationAsync(AcceptInvitationRequest request, CancellationToken cancellationToken = default);
}