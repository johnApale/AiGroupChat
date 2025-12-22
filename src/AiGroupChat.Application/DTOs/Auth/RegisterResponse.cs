namespace AiGroupChat.Application.DTOs.Auth;

/// <summary>
/// Response after user registration.
/// </summary>
/// <remarks>
/// Two possible outcomes:
/// 
/// 1. **Regular registration**: User must confirm email before logging in.
///    - RequiresEmailConfirmation = true
///    - Auth and GroupId are null
/// 
/// 2. **Invite-based registration**: Email is auto-confirmed, user is added to group.
///    - RequiresEmailConfirmation = false
///    - Auth contains tokens, GroupId contains the joined group
/// </remarks>
public class RegisterResponse
{
    /// <summary>
    /// Whether the user needs to confirm their email before logging in.
    /// False when registering via invitation link (email already verified).
    /// </summary>
    /// <example>true</example>
    public bool RequiresEmailConfirmation { get; set; }

    /// <summary>
    /// Human-readable message about registration status.
    /// </summary>
    /// <example>Registration successful. Please check your email to confirm your account.</example>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Auth tokens and user info. Only populated for invite-based registration.
    /// </summary>
    public AuthResponse? Auth { get; set; }

    /// <summary>
    /// Group ID the user was added to. Only populated for invite-based registration.
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    public Guid? GroupId { get; set; }
}