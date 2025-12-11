namespace AiGroupChat.Application.DTOs.Auth;

/// <summary>
/// Authentication response containing tokens and user info.
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// JWT access token. Include in Authorization header: "Bearer {token}"
    /// </summary>
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh token for obtaining new access tokens. Store securely.
    /// </summary>
    /// <example>a1b2c3d4-e5f6-7890-abcd-ef1234567890</example>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp when the access token expires.
    /// </summary>
    /// <example>2025-01-15T10:45:00Z</example>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Authenticated user's profile.
    /// </summary>
    public UserDto User { get; set; } = null!;
}

/// <summary>
/// User profile information.
/// </summary>
public class UserDto
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
}