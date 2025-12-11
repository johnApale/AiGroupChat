using AiGroupChat.Application.Interfaces;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Infrastructure.Data;

namespace AiGroupChat.Infrastructure.Repositories;

public class AiResponseMetadataRepository : IAiResponseMetadataRepository
{
    private readonly ApplicationDbContext _context;

    public AiResponseMetadataRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AiResponseMetadata> CreateAsync(AiResponseMetadata metadata, CancellationToken cancellationToken = default)
    {
        _context.AiResponseMetadata.Add(metadata);
        await _context.SaveChangesAsync(cancellationToken);
        return metadata;
    }
}