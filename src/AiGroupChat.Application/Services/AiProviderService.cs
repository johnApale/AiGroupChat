using AiGroupChat.Application.DTOs.AiProviders;
using AiGroupChat.Application.Exceptions;
using AiGroupChat.Application.Interfaces;
using AiGroupChat.Domain.Entities;

namespace AiGroupChat.Application.Services;

public class AiProviderService : IAiProviderService
{
    private readonly IAiProviderRepository _aiProviderRepository;

    public AiProviderService(IAiProviderRepository aiProviderRepository)
    {
        _aiProviderRepository = aiProviderRepository;
    }

    public async Task<List<AiProviderResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var providers = await _aiProviderRepository.GetAllEnabledAsync(cancellationToken);
        return providers.Select(MapToResponse).ToList();
    }

    public async Task<AiProviderResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var provider = await _aiProviderRepository.GetByIdAsync(id, cancellationToken);

        if (provider == null || !provider.IsEnabled)
        {
            throw new NotFoundException("AI Provider", id);
        }

        return MapToResponse(provider);
    }

    private static AiProviderResponse MapToResponse(AiProvider provider)
    {
        return new AiProviderResponse
        {
            Id = provider.Id,
            Name = provider.Name,
            DisplayName = provider.DisplayName,
            DefaultModel = provider.DefaultModel,
            DefaultTemperature = provider.DefaultTemperature,
            MaxTokensLimit = provider.MaxTokensLimit
        };
    }
}
