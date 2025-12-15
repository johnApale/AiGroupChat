using AiGroupChat.Application.DTOs.Auth;

namespace AiGroupChat.Application.DTOs.Invitations;

/// <summary>
/// Response after accepting an invitation.
/// </summary>
public class AcceptInvitationResponse
{
    /// <summary>
    /// Whether the user needs to register first.
    /// </summary>
    public bool RequiresRegistration { get; set; }

    /// <summary>
    /// Email address associated with the invitation (for registration form).
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Group name (for display during registration).
    /// </summary>
    public string? GroupName { get; set; }

    /// <summary>
    /// Auth response if user existed and was added to group.
    /// Null if registration is required.
    /// </summary>
    public AuthResponse? Auth { get; set; }

    /// <summary>
    /// Group ID the user was added to.
    /// Null if registration is required.
    /// </summary>
    public Guid? GroupId { get; set; }
}