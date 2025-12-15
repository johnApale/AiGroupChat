using System.Security.Claims;
using AiGroupChat.Application.DTOs.Invitations;
using AiGroupChat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiGroupChat.API.Controllers;

/// <summary>
/// Group invitation operations.
/// </summary>
/// <remarks>
/// Manage invitations to join a group via email.
/// 
/// **Flow:**
/// 1. Admin invites users by email
/// 2. Invitee receives email with link
/// 3. If invitee has account → added to group and logged in
/// 4. If invitee is new → redirected to signup, then added to group
/// 
/// **Permissions:**
/// | Action | Owner | Admin | Member |
/// |--------|-------|-------|--------|
/// | Send invitations | ✓ | ✓ | ✗ |
/// | View pending | ✓ | ✓ | ✗ |
/// | Revoke invitation | ✓ | ✓ | ✗ |
/// </remarks>
[ApiController]
[Route("api/groups/{groupId:guid}/invitations")]
[Tags("Group Invitations")]
[Authorize]
[Produces("application/json")]
public class GroupInvitationsController : ControllerBase
{
    private readonly IGroupInvitationService _invitationService;

    public GroupInvitationsController(IGroupInvitationService invitationService)
    {
        _invitationService = invitationService;
    }

    /// <summary>
    /// Invite members by email
    /// </summary>
    /// <remarks>
    /// Sends invitation emails to the specified addresses.
    /// 
    /// **Behavior:**
    /// - If email already has a pending invitation, it will be resent (timer resets)
    /// - If email belongs to an existing group member, it will be skipped
    /// - Each email receives an individual invitation (recipients don't see each other)
    /// 
    /// **Response includes:**
    /// - `sent` - Successfully sent invitations
    /// - `failed` - Emails that couldn't be invited with reasons
    /// </remarks>
    /// <param name="groupId">Group ID</param>
    /// <param name="request">List of email addresses to invite</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Results of the invitation batch</returns>
    /// <response code="200">Invitations processed (check sent/failed arrays)</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Not an admin of this group</response>
    /// <response code="404">Group not found</response>
    [HttpPost]
    [ProducesResponseType(typeof(InviteMembersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> InviteMembers(Guid groupId, [FromBody] InviteMembersRequest request, CancellationToken cancellationToken)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        InviteMembersResponse result = await _invitationService.InviteMembersAsync(groupId, request, userId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// List pending invitations
    /// </summary>
    /// <remarks>
    /// Returns all pending (not yet accepted or revoked) invitations for the group.
    /// Requires admin or owner role.
    /// </remarks>
    /// <param name="groupId">Group ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of pending invitations</returns>
    /// <response code="200">Invitations retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Not an admin of this group</response>
    /// <response code="404">Group not found</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<InvitationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPendingInvitations(Guid groupId, CancellationToken cancellationToken)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        List<InvitationResponse> result = await _invitationService.GetPendingInvitationsAsync(groupId, userId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Revoke an invitation
    /// </summary>
    /// <remarks>
    /// Cancels a pending invitation. The invitation link will no longer work.
    /// Only pending invitations can be revoked.
    /// Requires admin or owner role.
    /// </remarks>
    /// <param name="groupId">Group ID</param>
    /// <param name="invitationId">Invitation ID to revoke</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="204">Invitation revoked successfully</response>
    /// <response code="400">Invitation is not pending</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Not an admin of this group</response>
    /// <response code="404">Group or invitation not found</response>
    [HttpDelete("{invitationId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeInvitation(Guid groupId, Guid invitationId, CancellationToken cancellationToken)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await _invitationService.RevokeInvitationAsync(groupId, invitationId, userId, cancellationToken);
        return NoContent();
    }
}