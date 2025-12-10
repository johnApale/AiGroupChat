using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.AiProviders;

namespace AiGroupChat.IntegrationTests.Helpers;

/// <summary>
/// Helper methods for AI provider-related test operations
/// </summary>
public class AiProviderHelper
{
    private readonly HttpClient _client;

    public AiProviderHelper(HttpClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Gets all AI providers and returns the HTTP response
    /// </summary>
    public async Task<HttpResponseMessage> GetAllRawAsync()
    {
        return await _client.GetAsync("/api/ai-providers");
    }

    /// <summary>
    /// Gets all AI providers and returns the response list
    /// </summary>
    public async Task<List<AiProviderResponse>> GetAllAsync()
    {
        HttpResponseMessage response = await GetAllRawAsync();
        response.EnsureSuccessStatusCode();

        List<AiProviderResponse>? providers = await response.Content.ReadFromJsonAsync<List<AiProviderResponse>>();
        return providers ?? throw new InvalidOperationException("Failed to deserialize providers response");
    }

    /// <summary>
    /// Gets an AI provider by ID and returns the HTTP response
    /// </summary>
    public async Task<HttpResponseMessage> GetByIdRawAsync(Guid id)
    {
        return await _client.GetAsync($"/api/ai-providers/{id}");
    }

    /// <summary>
    /// Gets an AI provider by ID and returns the response object
    /// </summary>
    public async Task<AiProviderResponse> GetByIdAsync(Guid id)
    {
        HttpResponseMessage response = await GetByIdRawAsync(id);
        response.EnsureSuccessStatusCode();

        AiProviderResponse? provider = await response.Content.ReadFromJsonAsync<AiProviderResponse>();
        return provider ?? throw new InvalidOperationException("Failed to deserialize provider response");
    }
}