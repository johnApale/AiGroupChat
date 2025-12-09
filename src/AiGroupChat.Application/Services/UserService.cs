using AiGroupChat.Application.DTOs.Users;
using AiGroupChat.Application.Exceptions;
using AiGroupChat.Application.Interfaces;

namespace AiGroupChat.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserResponse> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        Domain.Entities.User? user = await _userRepository.FindByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User", userId);
        }

        return MapToResponse(user);
    }

    public async Task<UserResponse> GetCurrentUserAsync(string currentUserId, CancellationToken cancellationToken = default)
    {
        return await GetByIdAsync(currentUserId, cancellationToken);
    }

    private static UserResponse MapToResponse(Domain.Entities.User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            DisplayName = user.DisplayName,
            CreatedAt = user.CreatedAt
        };
    }
}