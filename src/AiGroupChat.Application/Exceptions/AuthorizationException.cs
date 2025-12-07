namespace AiGroupChat.Application.Exceptions;

/// <summary>
/// Thrown when a user lacks permission to perform an action
/// </summary>
public class AuthorizationException : Exception
{
    public AuthorizationException(string message) : base(message)
    {
    }
}