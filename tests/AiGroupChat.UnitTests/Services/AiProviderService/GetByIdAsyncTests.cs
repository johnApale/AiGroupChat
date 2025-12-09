using AiGroupChat.Application.DTOs.AiProviders;
using AiGroupChat.Application.Exceptions;
using AiGroupChat.Domain.Entities;
using Moq;

namespace AiGroupChat.UnitTests.Services.AiProviderService;

public class GetByIdAsyncTests : AiProviderServiceTestBase
{
    [Fact]
    public async Task WithValidId_ReturnsProvider()
    {
        // Arrange
        AiProvider provider = TestProviders[0];
        AiProviderRepositoryMock
            .Setup(x => x.GetByIdAsync(provider.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        AiProviderResponse result = await AiProviderService.GetByIdAsync(provider.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(provider.Id, result.Id);
        Assert.Equal(provider.Name, result.Name);
        Assert.Equal(provider.DisplayName, result.DisplayName);
    }

    [Fact]
    public async Task WithNonexistentId_ThrowsNotFoundException()
    {
        // Arrange
        Guid nonexistentId = Guid.NewGuid();
        AiProviderRepositoryMock
            .Setup(x => x.GetByIdAsync(nonexistentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AiProvider?)null);

        // Act & Assert
        NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
            () => AiProviderService.GetByIdAsync(nonexistentId));

        Assert.Contains("AI Provider", exception.Message);
    }

    [Fact]
    public async Task WithDisabledProvider_ThrowsNotFoundException()
    {
        // Arrange
        AiProvider disabledProvider = new AiProvider
        {
            Id = Guid.NewGuid(),
            Name = "disabled-provider",
            DisplayName = "Disabled Provider",
            IsEnabled = false,
            DefaultModel = "model",
            DefaultTemperature = 0.7m,
            MaxTokensLimit = 100000
        };

        AiProviderRepositoryMock
            .Setup(x => x.GetByIdAsync(disabledProvider.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(disabledProvider);

        // Act & Assert
        NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
            () => AiProviderService.GetByIdAsync(disabledProvider.Id));

        Assert.Contains("AI Provider", exception.Message);
    }
}
