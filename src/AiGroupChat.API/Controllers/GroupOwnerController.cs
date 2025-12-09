using System.Security.Claims;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiGroupChat.API.Controllers;

[ApiController]
[Route("api/groups/{groupId:guid}/owner")]
[Authorize]
public class GroupOwnerController : ControllerBase
{
    private readonly IGroupMemberService _groupMemberService;

    public GroupOwnerController(IGroupMemberService groupMemberService)
    {
        _groupMemberService = groupMemberService;
    }

    [HttpPut]
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