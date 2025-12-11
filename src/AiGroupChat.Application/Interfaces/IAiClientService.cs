using AiGroupChat.Application.DTOs.AiService;

namespace AiGroupChat.Application.Interfaces;

/// <summary>
/// Client for communicating with the Python AI service
/// </summary>
public interface IAiClientService
{
    /// <summary>
    /// Send a request to the AI service to generate a response
    /// </summary>
    /// <param name="request">The generation request with context and query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The AI response, or null if the service is unavailable</returns>
    Task<AiGenerateResponse?> GenerateAsync(AiGenerateRequest request, CancellationToken cancellationToken = default);
}