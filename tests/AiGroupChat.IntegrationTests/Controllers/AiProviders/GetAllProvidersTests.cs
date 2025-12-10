using System.Net;
using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.AiProviders;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Controllers.AiProviders;

public class GetAllProvidersTests : IntegrationTestBase
{
    public GetAllProvidersTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAll_ReturnsProviders()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();

        // Act
        HttpResponseMessage response = await AiProviders.GetAllRawAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<AiProviderResponse>? providers = await response.Content.ReadFromJsonAsync<List<AiProviderResponse>>();
        Assert.NotNull(providers);
        Assert.NotEmpty(providers);

        // Verify provider structure
        AiProviderResponse firstProvider = providers[0];
        Assert.NotEqual(Guid.Empty, firstProvider.Id);
        Assert.NotEmpty(firstProvider.Name);
        Assert.NotEmpty(firstProvider.DisplayName);
        Assert.NotEmpty(firstProvider.DefaultModel);
        Assert.True(firstProvider.DefaultTemperature >= 0);
        Assert.True(firstProvider.MaxTokensLimit > 0);
    }

    [Fact]
    public async Task GetAll_ReturnsExpectedProviders()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();

        // Act
        List<AiProviderResponse> providers = await AiProviders.GetAllAsync();

        // Assert - Should have seeded providers (at minimum Gemini for MVP)
        Assert.Contains(providers, p => p.Name == "gemini");
    }

    [Fact]
    public async Task GetAll_WithoutToken_Returns401()
    {
        // Arrange
        Auth.ClearAuthToken();

        // Act
        HttpResponseMessage response = await AiProviders.GetAllRawAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}