using System.Security.Claims;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiGroupChat.API.Controllers;

/// <summary>
/// Group management operations.
/// </summary>
/// <remarks>
/// Groups are the core collaboration unit. Each group has:
/// - **Owner** - Full control, can delete group and transfer ownership
/// - **Admins** - Can manage members and AI settings
/// - **Members** - Can send messages and view group content
/// </remarks>
[ApiController]
[Route("api/groups")]
[Tags("Groups")]
[Authorize]
[Produces("application/json")]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;

    public GroupsController(IGroupService groupService)
    {
        _groupService = groupService;
    }

    /// <summary>
    /// Create a new group
    /// </summary>
    /// <remarks>
    /// Creates a new group with the current user as the owner.
    /// The owner automatically has admin privileges.
    /// </remarks>
    /// <param name="request">Group details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created group with member list</returns>
    /// <response code="201">Group created successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">Not authenticated</response>
    [HttpPost]
    [ProducesResponseType(typeof(GroupResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateGroupRequest request, CancellationToken cancellationToken)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        GroupResponse result = await _groupService.CreateAsync(request, userId, cancellationToken);
        return StatusCode(201, result);
    }

    /// <summary>
    /// List my groups
    /// </summary>
    /// <remarks>
    /// Returns all groups where the current user is a member.
    /// Includes member list for each group.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of groups with members</returns>
    /// <response code="200">Groups retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<GroupResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyGroups(CancellationToken cancellationToken)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        List<GroupResponse> result = await _groupService.GetMyGroupsAsync(userId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get group by ID
    /// </summary>
    /// <remarks>
    /// Returns group details including all members.
    /// User must be a member of the group.
    /// </remarks>
    /// <param name="id">Group ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Group details with members</returns>
    /// <response code="200">Group found</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Not a member of this group</response>
    /// <response code="404">Group not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GroupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        GroupResponse result = await _groupService.GetByIdAsync(id, userId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Update group
    /// </summary>
    /// <remarks>
    /// Updates group details (currently only name).
    /// Requires admin or owner role.
    /// </remarks>
    /// <param name="id">Group ID</param>
    /// <param name="request">Updated group details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated group with members</returns>
    /// <response code="200">Group updated successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Not an admin of this group</response>
    /// <response code="404">Group not found</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(GroupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGroupRequest request, CancellationToken cancellationToken)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        GroupResponse result = await _groupService.UpdateAsync(id, request, userId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Delete group
    /// </summary>
    /// <remarks>
    /// Permanently deletes the group and all its messages.
    /// **This action cannot be undone.**
    /// Requires owner role.
    /// </remarks>
    /// <param name="id">Group ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="204">Group deleted successfully</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Not the owner of this group</response>
    /// <response code="404">Group not found</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await _groupService.DeleteAsync(id, userId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Update AI settings
    /// </summary>
    /// <remarks>
    /// Configure AI monitoring and provider for the group.
    /// 
    /// **AI Monitoring:**
    /// - When enabled, messages are visible to the AI for context
    /// - When disabled, new messages are hidden from AI
    /// - Only messages sent while monitoring is ON are included in AI context
    /// 
    /// **AI Provider:**
    /// - Select which AI provider to use (Gemini, Claude, etc.)
    /// - See `/api/ai-providers` for available options
    /// 
    /// Requires admin or owner role.
    /// </remarks>
    /// <param name="id">Group ID</param>
    /// <param name="request">AI settings to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated group with members</returns>
    /// <response code="200">AI settings updated successfully</response>
    /// <response code="400">Invalid provider ID</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Not an admin of this group</response>
    /// <response code="404">Group not found</response>
    [HttpPut("{id:guid}/ai")]
    [ProducesResponseType(typeof(GroupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAiSettings(Guid id, [FromBody] UpdateAiSettingsRequest request, CancellationToken cancellationToken)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        GroupResponse result = await _groupService.UpdateAiSettingsAsync(id, request, userId, cancellationToken);
        return Ok(result);
    }
}