namespace AiGroupChat.Application.DTOs.Invitations;

/// <summary>
/// Response after sending batch invitations.
/// </summary>
public class InviteMembersResponse
{
    /// <summary>
    /// Invitations that were successfully sent.
    /// </summary>
    public List<InvitationResponse> Sent { get; set; } = new();

    /// <summary>
    /// Emails that failed with reasons.
    /// </summary>
    public List<InvitationError> Failed { get; set; } = new();
}

public class InvitationError
{
    public string Email { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}