using AiGroupChat.Application.DTOs.Users;

namespace AiGroupChat.Application.Interfaces;

public interface IUserService
{
    /// <summary>
    /// Get user by their ID
    /// </summary>
    Task<UserResponse> GetByIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the currently authenticated user
    /// </summary>
    Task<UserResponse> GetCurrentUserAsync(string currentUserId, CancellationToken cancellationToken = default);
}