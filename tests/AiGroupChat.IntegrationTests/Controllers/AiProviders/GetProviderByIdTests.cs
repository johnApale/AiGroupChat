using System.Net;
using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.AiProviders;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Controllers.AiProviders;

public class GetProviderByIdTests : IntegrationTestBase
{
    public GetProviderByIdTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetById_WithValidId_ReturnsProvider()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();

        // First get all providers to get a valid ID
        List<AiProviderResponse> providers = await AiProviders.GetAllAsync();
        AiProviderResponse expectedProvider = providers[0];

        // Act
        HttpResponseMessage response = await AiProviders.GetByIdRawAsync(expectedProvider.Id);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        AiProviderResponse? provider = await response.Content.ReadFromJsonAsync<AiProviderResponse>();
        Assert.NotNull(provider);
        Assert.Equal(expectedProvider.Id, provider.Id);
        Assert.Equal(expectedProvider.Name, provider.Name);
        Assert.Equal(expectedProvider.DisplayName, provider.DisplayName);
        Assert.Equal(expectedProvider.DefaultModel, provider.DefaultModel);
        Assert.Equal(expectedProvider.DefaultTemperature, provider.DefaultTemperature);
        Assert.Equal(expectedProvider.MaxTokensLimit, provider.MaxTokensLimit);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_Returns404()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();

        Guid nonExistentId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await AiProviders.GetByIdRawAsync(nonExistentId);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithoutToken_Returns401()
    {
        // Arrange
        Auth.ClearAuthToken();

        Guid providerId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await AiProviders.GetByIdRawAsync(providerId);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}