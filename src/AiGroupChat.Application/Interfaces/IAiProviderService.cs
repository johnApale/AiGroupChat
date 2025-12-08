using AiGroupChat.Application.DTOs.AiProviders;

namespace AiGroupChat.Application.Interfaces;

public interface IAiProviderService
{
    Task<List<AiProviderResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AiProviderResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
