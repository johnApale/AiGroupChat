namespace AiGroupChat.Application.DTOs.Auth;

/// <summary>
/// Generic success message response.
/// </summary>
public class MessageResponse
{
    /// <summary>
    /// Human-readable success message.
    /// </summary>
    /// <example>Registration successful. Please check your email to confirm your account.</example>
    public string Message { get; set; } = string.Empty;

    public MessageResponse() { }

    public MessageResponse(string message)
    {
        Message = message;
    }
}