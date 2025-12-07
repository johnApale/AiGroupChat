namespace AiGroupChat.Application.Exceptions;

/// <summary>
/// Thrown when validation or business rules fail
/// </summary>
public class ValidationException : Exception
{
    public string[] Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = [message];
    }

    public ValidationException(string[] errors) : base(string.Join(", ", errors))
    {
        Errors = errors;
    }
}