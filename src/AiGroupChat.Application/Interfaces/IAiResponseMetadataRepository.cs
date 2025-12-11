
using AiGroupChat.Domain.Entities;

namespace AiGroupChat.Application.Interfaces;

public interface IAiResponseMetadataRepository
{
    /// <summary>
    /// Create AI response metadata for a message
    /// </summary>
    Task<AiResponseMetadata> CreateAsync(AiResponseMetadata metadata, CancellationToken cancellationToken = default);
}