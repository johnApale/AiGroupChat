using System.Security.Claims;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiGroupChat.API.Controllers;

[ApiController]
[Route("api/groups/{groupId:guid}/members")]
[Authorize]
public class GroupMembersController : ControllerBase
{
    private readonly IGroupMemberService _groupMemberService;

    public GroupMembersController(IGroupMemberService groupMemberService)
    {
        _groupMemberService = groupMemberService;
    }

    [HttpPost]
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

    [HttpGet]
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

    [HttpPut("{memberId}")]
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

    [HttpDelete("{memberId}")]
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

    [HttpDelete("me")]
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