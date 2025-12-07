namespace AiGroupChat.Email.Models;

public class EmailResult
{
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// Email ID returned by the provider (useful for tracking)
    /// </summary>
    public string? EmailId { get; set; }
    
    /// <summary>
    /// Error message if sending failed
    /// </summary>
    public string? Error { get; set; }
    
    public static EmailResult Success(string emailId) => new()
    {
        IsSuccess = true,
        EmailId = emailId
    };
    
    public static EmailResult Failure(string error) => new()
    {
        IsSuccess = false,
        Error = error
    };
}