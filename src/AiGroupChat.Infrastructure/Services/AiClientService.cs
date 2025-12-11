using System.Net.Http.Json;
using System.Text.Json;
using AiGroupChat.Application.DTOs.AiService;
using AiGroupChat.Application.Interfaces;
using AiGroupChat.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiGroupChat.Infrastructure.Services;

public class AiClientService : IAiClientService
{
    private readonly HttpClient _httpClient;
    private readonly AiServiceSettings _settings;
    private readonly ILogger<AiClientService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public AiClientService(
        HttpClient httpClient,
        IOptions<AiServiceSettings> settings,
        ILogger<AiClientService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<AiGenerateResponse?> GenerateAsync(AiGenerateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Calling AI service for provider {Provider} with {ContextCount} context messages",
                request.Provider,
                request.Context.Count);

            using HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, "/generate");
            httpRequest.Headers.Add("X-API-Key", _settings.ApiKey);
            httpRequest.Content = JsonContent.Create(request, options: JsonOptions);

            using HttpResponseMessage response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "AI service returned {StatusCode}: {Error}",
                    response.StatusCode,
                    errorBody);
                return null;
            }

            AiGenerateResponse? result = await response.Content.ReadFromJsonAsync<AiGenerateResponse>(JsonOptions, cancellationToken);

            if (result != null)
            {
                _logger.LogInformation(
                    "AI service responded successfully. Model: {Model}, Tokens: {Input}/{Output}, Latency: {Latency}ms",
                    result.Metadata.Model,
                    result.Metadata.TokensInput,
                    result.Metadata.TokensOutput,
                    result.Metadata.LatencyMs);
            }

            return result;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError("AI service request timed out after {Timeout} seconds", _settings.TimeoutSeconds);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to AI service at {BaseUrl}", _settings.BaseUrl);
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize AI service response");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling AI service");
            return null;
        }
    }
}