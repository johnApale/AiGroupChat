namespace AiGroupChat.Application.DTOs.Users;

/// <summary>
/// User profile details.
/// </summary>
public class UserResponse
{
    /// <summary>
    /// Unique user identifier.
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// User's email address.
    /// </summary>
    /// <example>john.doe@example.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Unique username.
    /// </summary>
    /// <example>johndoe</example>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Display name shown in the UI.
    /// </summary>
    /// <example>John Doe</example>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// When the account was created.
    /// </summary>
    /// <example>2025-01-10T08:00:00Z</example>
    public DateTime CreatedAt { get; set; }
}