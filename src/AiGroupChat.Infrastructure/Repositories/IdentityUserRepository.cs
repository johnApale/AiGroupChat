using AiGroupChat.Application.Interfaces;
using AiGroupChat.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AiGroupChat.Infrastructure.Repositories;

public class IdentityUserRepository : IUserRepository
{
    private readonly UserManager<User> _userManager;

    public IdentityUserRepository(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _userManager.FindByIdAsync(userId);
    }

    public async Task<(bool Succeeded, string[] Errors)> CreateAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.CreateAsync(user, password);
        var errors = result.Errors.Select(e => e.Description).ToArray();
        return (result.Succeeded, errors);
    }

    public async Task<bool> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<bool> IsEmailConfirmedAsync(User user, CancellationToken cancellationToken = default)
    {
        return await _userManager.IsEmailConfirmedAsync(user);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(User user, CancellationToken cancellationToken = default)
    {
        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<bool> ConfirmEmailAsync(User user, string token, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }

    public async Task<string> GeneratePasswordResetTokenAsync(User user, CancellationToken cancellationToken = default)
    {
        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<(bool Succeeded, string[] Errors)> ResetPasswordAsync(User user, string token, string newPassword, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        var errors = result.Errors.Select(e => e.Description).ToArray();
        return (result.Succeeded, errors);
    }
}