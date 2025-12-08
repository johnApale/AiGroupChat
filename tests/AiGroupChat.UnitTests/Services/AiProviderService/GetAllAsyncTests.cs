using Moq;

namespace AiGroupChat.UnitTests.Services.AiProviderService;

public class GetAllAsyncTests : AiProviderServiceTestBase
{
    [Fact]
    public async Task WithEnabledProviders_ReturnsAllProviders()
    {
        // Arrange
        AiProviderRepositoryMock
            .Setup(x => x.GetAllEnabledAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestProviders);

        // Act
        var result = await AiProviderService.GetAllAsync();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("gemini", result[0].Name);
        Assert.Equal("claude", result[1].Name);
        Assert.Equal("openai", result[2].Name);
    }

    [Fact]
    public async Task WithNoProviders_ReturnsEmptyList()
    {
        // Arrange
        AiProviderRepositoryMock
            .Setup(x => x.GetAllEnabledAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.AiProvider>());

        // Act
        var result = await AiProviderService.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ReturnsCorrectDtoMapping()
    {
        // Arrange
        var provider = TestProviders[0];
        AiProviderRepositoryMock
            .Setup(x => x.GetAllEnabledAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.AiProvider> { provider });

        // Act
        var result = await AiProviderService.GetAllAsync();

        // Assert
        Assert.Single(result);
        var dto = result[0];
        Assert.Equal(provider.Id, dto.Id);
        Assert.Equal(provider.Name, dto.Name);
        Assert.Equal(provider.DisplayName, dto.DisplayName);
        Assert.Equal(provider.DefaultModel, dto.DefaultModel);
        Assert.Equal(provider.DefaultTemperature, dto.DefaultTemperature);
        Assert.Equal(provider.MaxTokensLimit, dto.MaxTokensLimit);
    }
}
