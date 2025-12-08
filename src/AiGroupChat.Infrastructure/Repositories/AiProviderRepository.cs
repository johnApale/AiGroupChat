using AiGroupChat.Application.Interfaces;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiGroupChat.Infrastructure.Repositories;

public class AiProviderRepository : IAiProviderRepository
{
    private readonly ApplicationDbContext _context;

    public AiProviderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AiProvider>> GetAllEnabledAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AiProviders
            .Where(p => p.IsEnabled)
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<AiProvider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AiProviders
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<AiProvider?> GetDefaultAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AiProviders
            .Where(p => p.IsEnabled)
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Name)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
