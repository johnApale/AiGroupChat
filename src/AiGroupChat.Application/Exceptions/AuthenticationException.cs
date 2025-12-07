namespace AiGroupChat.Application.Exceptions;

/// <summary>
/// Thrown when authentication fails (invalid credentials, unconfirmed email, etc.)
/// </summary>
public class AuthenticationException : Exception
{
    public AuthenticationException(string message) : base(message)
    {
    }
}