namespace AiGroupChat.Application.Configuration;

public class InvitationSettings
{
    public const string SectionName = "Invitation";

    /// <summary>
    /// Number of days until invitation expires
    /// </summary>
    public int ExpirationDays { get; set; } = 7;
}