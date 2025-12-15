using AiGroupChat.Application.DTOs.Invitations;
using AiGroupChat.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AiGroupChat.API.Controllers;

/// <summary>
/// Public invitation acceptance endpoint.
/// </summary>
/// <remarks>
/// This endpoint is publicly accessible (no authentication required)
/// because users clicking invitation links may not have accounts yet.
/// </remarks>
[ApiController]
[Route("api/invitations")]
[Tags("Invitations")]
[Produces("application/json")]
public class InvitationsController : ControllerBase
{
    private readonly IGroupInvitationService _invitationService;

    public InvitationsController(IGroupInvitationService invitationService)
    {
        _invitationService = invitationService;
    }

    /// <summary>
    /// Accept an invitation
    /// </summary>
    /// <remarks>
    /// Accepts a group invitation using the token from the email link.
    /// 
    /// **Two possible outcomes:**
    /// 
    /// 1. **User exists** (email matches an account):
    ///    - User is added to the group
    ///    - Returns auth tokens (user is logged in)
    ///    - `requiresRegistration: false`
    /// 
    /// 2. **User doesn't exist**:
    ///    - Returns group info for display
    ///    - `requiresRegistration: true`
    ///    - Frontend should redirect to signup with the token
    /// 
    /// **Token validation:**
    /// - Token must be valid and not expired
    /// - Invitation must be in Pending status
    /// </remarks>
    /// <param name="request">Invitation token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Accept result with auth or registration requirement</returns>
    /// <response code="200">Invitation processed successfully</response>
    /// <response code="400">Invalid token, expired, or already used</response>
    /// <response code="404">Invitation not found</response>
    [HttpPost("accept")]
    [ProducesResponseType(typeof(AcceptInvitationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationRequest request, CancellationToken cancellationToken)
    {
        AcceptInvitationResponse result = await _invitationService.AcceptInvitationAsync(request, cancellationToken);
        return Ok(result);
    }
}