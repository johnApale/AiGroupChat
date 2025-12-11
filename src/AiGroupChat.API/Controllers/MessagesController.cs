using System.Security.Claims;
using AiGroupChat.Application.DTOs.Common;
using AiGroupChat.Application.DTOs.Messages;
using AiGroupChat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiGroupChat.API.Controllers;

/// <summary>
/// Message operations.
/// </summary>
/// <remarks>
/// Send and retrieve messages in groups.
/// 
/// **Message delivery:**
/// - Messages are sent via REST API (this controller)
/// - Messages are received in real-time via SignalR (`MessageReceived` event)
/// - AI responses are delivered via SignalR (`AiResponseReceived` event)
/// 
/// **AI Invocation:**
/// To invoke the AI, mention it in your message using `@` followed by the provider name:
/// - `@gemini what do you think?`
/// - `@claude can you help with this?`
/// 
/// The AI only sees messages sent while AI monitoring was enabled for the group.
/// </remarks>
[ApiController]
[Route("api/groups/{groupId:guid}/messages")]
[Tags("Messages")]
[Authorize]
[Produces("application/json")]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    /// <summary>
    /// Send a message
    /// </summary>
    /// <remarks>
    /// Sends a message to the group.
    /// 
    /// **Real-time delivery:**
    /// All group members connected via SignalR will receive the message instantly
    /// via the `MessageReceived` event.
    /// 
    /// **AI Invocation:**
    /// If the message contains an @mention of an AI provider (e.g., `@gemini`),
    /// and AI monitoring is enabled for the group, the AI will be invoked.
    /// 
    /// The AI response will be delivered via SignalR:
    /// 1. `AiTyping` event when AI starts generating
    /// 2. `AiResponseReceived` event with the AI's message
    /// 3. `AiStoppedTyping` event when complete
    /// </remarks>
    /// <param name="groupId">Group ID</param>
    /// <param name="request">Message content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The sent message</returns>
    /// <response code="201">Message sent successfully</response>
    /// <response code="400">Validation error (empty message, too long, etc.)</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Not a member of this group</response>
    /// <response code="404">Group not found</response>
    [HttpPost]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Get message history
    /// </summary>
    /// <remarks>
    /// Retrieves paginated message history for the group.
    /// Messages are returned in reverse chronological order (newest first).
    /// 
    /// **Pagination:**
    /// - Default page size is 50 messages
    /// - Use `page` parameter to load older messages
    /// - Response includes `totalCount`, `totalPages`, and navigation flags
    /// 
    /// **Includes:**
    /// - User messages with sender details
    /// - AI responses with provider information
    /// - Attachment metadata (if any)
    /// </remarks>
    /// <param name="groupId">Group ID</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Messages per page (default: 50, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of messages</returns>
    /// <response code="200">Messages retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Not a member of this group</response>
    /// <response code="404">Group not found</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<MessageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMessages(
        Guid groupId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        PaginatedResponse<MessageResponse> result = await _messageService.GetMessagesAsync(groupId, userId, page, pageSize, cancellationToken);
        return Ok(result);
    }
}