using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.Exceptions;
using AiGroupChat.Application.Interfaces;
using AiGroupChat.Application.Models;
using AiGroupChat.Domain.Entities;

namespace AiGroupChat.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;

    public AuthService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _emailService = emailService;
    }

    public async Task<MessageResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        User user = new User
        {
            Email = request.Email,
            UserName = request.UserName,
            DisplayName = request.DisplayName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        (bool succeeded, string[] errors) = await _userRepository.CreateAsync(user, request.Password, cancellationToken);

        if (!succeeded)
        {
            throw new ValidationException(errors);
        }

        string token = await _userRepository.GenerateEmailConfirmationTokenAsync(user, cancellationToken);
        await _emailService.SendConfirmationEmailAsync(user.Email!, user.DisplayName, token, cancellationToken);

        return new MessageResponse("Registration successful. Please check your email to confirm your account.");
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        User? user = await _userRepository.FindByEmailAsync(request.Email, cancellationToken);

        if (user == null)
        {
            throw new AuthenticationException("Invalid email or password.");
        }

        bool isEmailConfirmed = await _userRepository.IsEmailConfirmedAsync(user, cancellationToken);

        if (!isEmailConfirmed)
        {
            throw new AuthenticationException("Please confirm your email before logging in.");
        }

        bool isPasswordValid = await _userRepository.CheckPasswordAsync(user, request.Password, cancellationToken);

        if (!isPasswordValid)
        {
            throw new AuthenticationException("Invalid email or password.");
        }

        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default)
    {
        User? user = await _userRepository.FindByEmailAsync(request.Email, cancellationToken);

        if (user == null)
        {
            throw new ValidationException("Invalid confirmation request.");
        }

        bool succeeded = await _userRepository.ConfirmEmailAsync(user, request.Token, cancellationToken);

        if (!succeeded)
        {
            throw new ValidationException("Invalid or expired confirmation token.");
        }

        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<MessageResponse> ResendConfirmationAsync(ResendConfirmationRequest request, CancellationToken cancellationToken = default)
    {
        User? user = await _userRepository.FindByEmailAsync(request.Email, cancellationToken);

        // Always return success to prevent email enumeration
        if (user == null)
        {
            return new MessageResponse("If an unconfirmed account exists with this email, a confirmation link has been sent.");
        }

        bool isEmailConfirmed = await _userRepository.IsEmailConfirmedAsync(user, cancellationToken);

        if (!isEmailConfirmed)
        {
            string token = await _userRepository.GenerateEmailConfirmationTokenAsync(user, cancellationToken);
            await _emailService.SendConfirmationEmailAsync(user.Email!, user.DisplayName, token, cancellationToken);
        }

        return new MessageResponse("If an unconfirmed account exists with this email, a confirmation link has been sent.");
    }

    public async Task<MessageResponse> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        User? user = await _userRepository.FindByEmailAsync(request.Email, cancellationToken);

        // Always return success to prevent email enumeration
        if (user != null)
        {
            string token = await _userRepository.GeneratePasswordResetTokenAsync(user, cancellationToken);
            await _emailService.SendPasswordResetEmailAsync(user.Email!, user.DisplayName, token, cancellationToken);
        }

        return new MessageResponse("If an account exists with this email, a password reset link has been sent.");
    }

    public async Task<MessageResponse> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        User? user = await _userRepository.FindByEmailAsync(request.Email, cancellationToken);

        if (user == null)
        {
            throw new ValidationException("Invalid password reset request.");
        }

        (bool succeeded, string[] errors) = await _userRepository.ResetPasswordAsync(user, request.Token, request.NewPassword, cancellationToken);

        if (!succeeded)
        {
            throw new ValidationException(errors);
        }

        // Revoke all refresh tokens after password reset
        await _tokenService.RevokeAllUserRefreshTokensAsync(user.Id, cancellationToken);

        return new MessageResponse("Password has been reset successfully. You can now log in with your new password.");
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        string? userId = await _tokenService.ValidateRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (userId == null)
        {
            throw new AuthenticationException("Invalid or expired refresh token.");
        }

        User? user = await _userRepository.FindByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            throw new AuthenticationException("User not found.");
        }

        // Revoke old refresh token
        await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);

        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<MessageResponse> LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default)
    {
        await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);

        return new MessageResponse("Logged out successfully.");
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(User user, CancellationToken cancellationToken)
    {
        string accessToken = _tokenService.GenerateAccessToken(user);
        string refreshToken = await _tokenService.GenerateRefreshTokenAsync(user, cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = _tokenService.GetAccessTokenExpiration(),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                DisplayName = user.DisplayName
            }
        };
    }
}