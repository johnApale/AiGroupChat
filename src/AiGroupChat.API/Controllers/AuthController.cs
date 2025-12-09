using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AiGroupChat.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        MessageResponse result = await _authService.RegisterAsync(request, cancellationToken);
        return StatusCode(201, result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        AuthResponse result = await _authService.LoginAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        AuthResponse result = await _authService.ConfirmEmailAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationRequest request, CancellationToken cancellationToken)
    {
        MessageResponse result = await _authService.ResendConfirmationAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        MessageResponse result = await _authService.ForgotPasswordAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        MessageResponse result = await _authService.ResetPasswordAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        AuthResponse result = await _authService.RefreshTokenAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        MessageResponse result = await _authService.LogoutAsync(request, cancellationToken);
        return Ok(result);
    }
}