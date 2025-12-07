namespace AiGroupChat.Infrastructure.Configuration;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>
    /// Secret key for signing tokens (min 32 characters)
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Token issuer (typically your app name or domain)
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Token audience (typically your app name or domain)
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Access token lifetime in minutes
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    /// <summary>
    /// Refresh token lifetime in days
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;
}