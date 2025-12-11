using System.Security.Claims;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiGroupChat.API.Controllers;

/// <summary>
/// Group membership operations.
/// </summary>
/// <remarks>
/// Manage group members and their roles.
/// 
/// **Role hierarchy:**
/// - **Owner** - Can do everything, including delete group and transfer ownership
/// - **Admin** - Can add/remove members (except owner), update AI settings
/// - **Member** - Can view group and send messages
/// 
/// **Permissions:**
/// | Action | Owner | Admin | Member |
/// |--------|-------|-------|--------|
/// | Add member | ✓ | ✓ | ✗ |
/// | Remove member | ✓ | ✓* | ✗ |
/// | Change role | ✓ | ✗ | ✗ |
/// | Leave group | ✗** | ✓ | ✓ |
/// 
/// *Admins can only remove Members, not other Admins or Owner
/// **Owner must transfer ownership before leaving
/// </remarks>
[ApiController]
[Route("api/groups/{groupId:guid}/members")]
[Tags("Group Members")]
[Authorize]
[Produces("application/json")]
public class GroupMembersController : ControllerBase
{
    private readonly IGroupMemberService _groupMemberService;

    public GroupMembersController(IGroupMemberService groupMemberService)
    {
        _groupMemberService = groupMemberService;
    }

    /// <summary>
    /// Add member to group
    /// </summary>
    /// <remarks>
    /// Adds a user to the group with the Member role.
    /// Requires admin or owner role.
    /// 
    /// The new member will receive:
    /// - SignalR notification (`AddedToGroup` event)
    /// - Access to group messages sent while AI monitoring was enabled
    /// </remarks>
    /// <param name="groupId">Group ID</param>
    /// <param name="request">User ID to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The new member details</returns>
    /// <response code="201">Member added successfully</response>
    /// <response code="400">User is already a member</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Not an admin of this group</response>
    /// <response code="404">Group or user not found</response>
    [HttpPost]
    [ProducesResponseType(typeof(GroupMemberResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddMember(Guid groupId, [FromBody] AddMemberRequest request, CancellationToken cancellationToken)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        GroupMemberResponse result = await _groupMemberService.AddMemberAsync(groupId, request, userId, cancellationToken);
        return StatusCode(201, result);
    }

    /// <summary>
    /// List group members
    /// </summary>
    /// <remarks>
    /// Returns all members of the group with their roles.
    /// User must be a member of the group.
    /// </remarks>
    /// <param name="groupId">Group ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of group members</returns>
    /// <response code="200">Members retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Not a member of this group</response>
    /// <response code="404">Group not found</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<GroupMemberResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMembers(Guid groupId, CancellationToken cancellationToken)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        List<GroupMemberResponse> result = await _groupMemberService.GetMembersAsync(groupId, userId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Update member role
    /// </summary>
    /// <remarks>
    /// Changes a member's role within the group.
    /// 
    /// **Rules:**
    /// - Only the owner can change roles
    /// - Cannot change the owner's role (use transfer ownership instead)
    /// - Valid roles: `Admin`, `Member`
    /// 
    /// The affected member will receive a SignalR notification (`RoleChanged` event).
    /// </remarks>
    /// <param name="groupId">Group ID</param>
    /// <param name="memberId">User ID of the member to update</param>
    /// <param name="request">New role</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated member details</returns>
    /// <response code="200">Role updated successfully</response>
    /// <response code="400">Invalid role or cannot change owner's role</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Not the owner of this group</response>
    /// <response code="404">Group or member not found</response>
    [HttpPut("{memberId}")]
    [ProducesResponseType(typeof(GroupMemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMemberRole(Guid groupId, string memberId, [FromBody] UpdateMemberRoleRequest request, CancellationToken cancellationToken)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        GroupMemberResponse result = await _groupMemberService.UpdateMemberRoleAsync(groupId, memberId, request, userId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Remove member from group
    /// </summary>
    /// <remarks>
    /// Removes a user from the group.
    /// 
    /// **Rules:**
    /// - Owner can remove anyone except themselves
    /// - Admin can only remove Members (not other Admins or Owner)
    /// - Use `DELETE /me` to leave the group yourself
    /// 
    /// The removed member will receive a SignalR notification (`RemovedFromGroup` event).
    /// </remarks>
    /// <param name="groupId">Group ID</param>
    /// <param name="memberId">User ID of the member to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="204">Member removed successfully</response>
    /// <response code="400">Cannot remove yourself (use leave endpoint)</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Insufficient permissions to remove this member</response>
    /// <response code="404">Group or member not found</response>
    [HttpDelete("{memberId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMember(Guid groupId, string memberId, CancellationToken cancellationToken)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await _groupMemberService.RemoveMemberAsync(groupId, memberId, userId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Leave group
    /// </summary>
    /// <remarks>
    /// Removes the current user from the group.
    /// 
    /// **Rules:**
    /// - Owner cannot leave (must transfer ownership first)
    /// - Admins and Members can leave freely
    /// 
    /// Other group members will see a SignalR notification (`MemberLeft` event).
    /// </remarks>
    /// <param name="groupId">Group ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="204">Left group successfully</response>
    /// <response code="400">Owner cannot leave (transfer ownership first)</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="404">Group not found or not a member</response>
    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LeaveGroup(Guid groupId, CancellationToken cancellationToken)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await _groupMemberService.LeaveGroupAsync(groupId, userId, cancellationToken);
        return NoContent();
    }
}