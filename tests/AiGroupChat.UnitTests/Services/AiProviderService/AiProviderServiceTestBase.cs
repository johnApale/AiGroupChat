using AiGroupChat.Application.Interfaces;
using AiGroupChat.Domain.Entities;
using Moq;

namespace AiGroupChat.UnitTests.Services.AiProviderService;

public abstract class AiProviderServiceTestBase
{
    protected readonly Mock<IAiProviderRepository> AiProviderRepositoryMock;
    protected readonly Application.Services.AiProviderService AiProviderService;

    protected readonly List<AiProvider> TestProviders = new()
    {
        new AiProvider
        {
            Id = Guid.NewGuid(),
            Name = "gemini",
            DisplayName = "Google Gemini",
            IsEnabled = true,
            SortOrder = 0,
            DefaultModel = "gemini-1.5-pro",
            DefaultTemperature = 0.7m,
            MaxTokensLimit = 1000000
        },
        new AiProvider
        {
            Id = Guid.NewGuid(),
            Name = "claude",
            DisplayName = "Anthropic Claude",
            IsEnabled = true,
            SortOrder = 1,
            DefaultModel = "claude-3-5-sonnet",
            DefaultTemperature = 0.7m,
            MaxTokensLimit = 200000
        },
        new AiProvider
        {
            Id = Guid.NewGuid(),
            Name = "openai",
            DisplayName = "OpenAI",
            IsEnabled = true,
            SortOrder = 2,
            DefaultModel = "gpt-4o",
            DefaultTemperature = 0.7m,
            MaxTokensLimit = 128000
        }
    };

    protected AiProviderServiceTestBase()
    {
        AiProviderRepositoryMock = new Mock<IAiProviderRepository>();
        AiProviderService = new Application.Services.AiProviderService(AiProviderRepositoryMock.Object);
    }
}
