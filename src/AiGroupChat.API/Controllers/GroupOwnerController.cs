using System.Security.Claims;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiGroupChat.API.Controllers;

/// <summary>
/// Group ownership operations.
/// </summary>
[ApiController]
[Route("api/groups/{groupId:guid}/owner")]
[Tags("Group Members")]
[Authorize]
[Produces("application/json")]
public class GroupOwnerController : ControllerBase
{
    private readonly IGroupMemberService _groupMemberService;

    public GroupOwnerController(IGroupMemberService groupMemberService)
    {
        _groupMemberService = groupMemberService;
    }

    /// <summary>
    /// Transfer group ownership
    /// </summary>
    /// <remarks>
    /// Transfers ownership of the group to another member.
    /// 
    /// **What happens:**
    /// - New owner gets the Owner role
    /// - Current owner is demoted to Admin
    /// - Both users receive SignalR notifications (`RoleChanged` event)
    /// 
    /// **Rules:**
    /// - Only the current owner can transfer ownership
    /// - New owner must already be a member of the group
    /// - This is the only way for an owner to leave the group
    /// </remarks>
    /// <param name="groupId">Group ID</param>
    /// <param name="request">User ID of the new owner</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New owner's member details</returns>
    /// <response code="200">Ownership transferred successfully</response>
    /// <response code="400">Cannot transfer to yourself or invalid user</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Not the owner of this group</response>
    /// <response code="404">Group or target user not found</response>
    [HttpPut]
    [ProducesResponseType(typeof(GroupMemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransferOwnership(Guid groupId, [FromBody] TransferOwnershipRequest request, CancellationToken cancellationToken)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        GroupMemberResponse result = await _groupMemberService.TransferOwnershipAsync(groupId, request, userId, cancellationToken);
        return Ok(result);
    }
}