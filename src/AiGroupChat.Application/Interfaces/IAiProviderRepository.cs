using AiGroupChat.Domain.Entities;

namespace AiGroupChat.Application.Interfaces;

public interface IAiProviderRepository
{
    Task<IEnumerable<AiProvider>> GetAllEnabledAsync(CancellationToken cancellationToken = default);
    Task<AiProvider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AiProvider?> GetDefaultAsync(CancellationToken cancellationToken = default);
}
