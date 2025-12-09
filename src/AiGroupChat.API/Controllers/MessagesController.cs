using System.Security.Claims;
using AiGroupChat.Application.DTOs.Messages;
using AiGroupChat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiGroupChat.API.Controllers;

[ApiController]
[Route("api/groups/{groupId:guid}/messages")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage(Guid groupId, [FromBody] SendMessageRequest request, CancellationToken cancellationToken)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        MessageResponse result = await _messageService.SendMessageAsync(groupId, request, userId, cancellationToken);
        return StatusCode(201, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetMessages(Guid groupId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _messageService.GetMessagesAsync(groupId, userId, page, pageSize, cancellationToken);
        return Ok(result);
    }
}
