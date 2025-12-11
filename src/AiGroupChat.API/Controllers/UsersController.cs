using System.Security.Claims;
using AiGroupChat.Application.DTOs.Users;
using AiGroupChat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiGroupChat.API.Controllers;

/// <summary>
/// User profile operations.
/// </summary>
[ApiController]
[Route("api/users")]
[Tags("Users")]
[Authorize]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Get current authenticated user
    /// </summary>
    /// <remarks>
    /// Returns the profile of the currently authenticated user.
    /// Useful for fetching user details after login or page refresh.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current user's profile</returns>
    /// <response code="200">User profile retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        UserResponse result = await _userService.GetCurrentUserAsync(userId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <remarks>
    /// Returns the public profile of any user by their ID.
    /// Used for displaying member information in groups.
    /// </remarks>
    /// <param name="id">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User's public profile</returns>
    /// <response code="200">User found</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="404">User not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        UserResponse result = await _userService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }
}