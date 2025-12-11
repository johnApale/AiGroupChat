using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AiGroupChat.API.Controllers;

/// <summary>
/// Handles user authentication, registration, and account management.
/// </summary>
[ApiController]
[Route("api/auth")]
[Tags("Authentication")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <remarks>
    /// Creates a new user account and sends a confirmation email.
    /// The user must confirm their email before they can log in.
    /// 
    /// **Password requirements:**
    /// - At least 6 characters
    /// - At least one uppercase letter
    /// - At least one lowercase letter
    /// - At least one digit
    /// - At least one non-alphanumeric character
    /// </remarks>
    /// <param name="request">Registration details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    /// <response code="201">Account created successfully. Check email for confirmation link.</response>
    /// <response code="400">Validation error (invalid email, weak password, etc.)</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        MessageResponse result = await _authService.RegisterAsync(request, cancellationToken);
        return StatusCode(201, result);
    }

    /// <summary>
    /// Log in to an existing account
    /// </summary>
    /// <remarks>
    /// Authenticates the user and returns JWT access and refresh tokens.
    /// 
    /// **Token usage:**
    /// - Access token expires in 15 minutes
    /// - Include in requests: `Authorization: Bearer {accessToken}`
    /// - Use refresh token to get new access token when expired
    /// </remarks>
    /// <param name="request">Login credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Access token, refresh token, and user details</returns>
    /// <response code="200">Login successful</response>
    /// <response code="401">Invalid credentials or email not confirmed</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        AuthResponse result = await _authService.LoginAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Confirm email address
    /// </summary>
    /// <remarks>
    /// Verifies the user's email using the token sent during registration.
    /// On success, returns tokens so the user is automatically logged in.
    /// </remarks>
    /// <param name="request">Email and confirmation token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Access token, refresh token, and user details</returns>
    /// <response code="200">Email confirmed and user logged in</response>
    /// <response code="400">Invalid or expired token</response>
    [HttpPost("confirm-email")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        AuthResponse result = await _authService.ConfirmEmailAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Resend confirmation email
    /// </summary>
    /// <remarks>
    /// Sends a new confirmation email if the account exists and is not yet confirmed.
    /// Always returns success to prevent email enumeration attacks.
    /// </remarks>
    /// <param name="request">Email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    /// <response code="200">If account exists and is unconfirmed, email was sent</response>
    [HttpPost("resend-confirmation")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationRequest request, CancellationToken cancellationToken)
    {
        MessageResponse result = await _authService.ResendConfirmationAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Request password reset
    /// </summary>
    /// <remarks>
    /// Sends a password reset email if the account exists.
    /// Always returns success to prevent email enumeration attacks.
    /// Reset link expires in 1 hour.
    /// </remarks>
    /// <param name="request">Email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    /// <response code="200">If account exists, reset email was sent</response>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        MessageResponse result = await _authService.ForgotPasswordAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    /// <remarks>
    /// Sets a new password using the token from the reset email.
    /// All existing refresh tokens are revoked after password reset.
    /// </remarks>
    /// <param name="request">Email, reset token, and new password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    /// <response code="200">Password reset successful</response>
    /// <response code="400">Invalid or expired token</response>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        MessageResponse result = await _authService.ResetPasswordAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    /// <remarks>
    /// Exchanges a valid refresh token for a new access token and refresh token.
    /// The old refresh token is revoked (rotation for security).
    /// </remarks>
    /// <param name="request">Current refresh token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New access token and refresh token</returns>
    /// <response code="200">Tokens refreshed successfully</response>
    /// <response code="401">Invalid or expired refresh token</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        AuthResponse result = await _authService.RefreshTokenAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Log out and revoke refresh token
    /// </summary>
    /// <remarks>
    /// Revokes the provided refresh token, preventing it from being used again.
    /// The access token remains valid until it expires (15 minutes).
    /// </remarks>
    /// <param name="request">Refresh token to revoke</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    /// <response code="200">Logout successful</response>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        MessageResponse result = await _authService.LogoutAsync(request, cancellationToken);
        return Ok(result);
    }
}