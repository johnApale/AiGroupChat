using System.Net;
using System.Text.Json;
using AiGroupChat.Application.DTOs.AiService;
using AiGroupChat.Infrastructure.Configuration;
using AiGroupChat.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace AiGroupChat.UnitTests.Services.AiClientService;

public abstract class AiClientServiceTestBase
{
    protected readonly Mock<HttpMessageHandler> HttpMessageHandlerMock;
    protected readonly HttpClient HttpClient;
    protected readonly Mock<IOptions<AiServiceSettings>> SettingsMock;
    protected readonly Mock<ILogger<Infrastructure.Services.AiClientService>> LoggerMock;
    protected readonly Infrastructure.Services.AiClientService AiClientService;
    protected readonly AiServiceSettings Settings;

    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    protected AiClientServiceTestBase()
    {
        HttpMessageHandlerMock = new Mock<HttpMessageHandler>();
        
        Settings = new AiServiceSettings
        {
            BaseUrl = "http://localhost:8000",
            ApiKey = "test-api-key",
            TimeoutSeconds = 30,
            MaxContextMessages = 100
        };

        HttpClient = new HttpClient(HttpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(Settings.BaseUrl)
        };

        SettingsMock = new Mock<IOptions<AiServiceSettings>>();
        SettingsMock.Setup(x => x.Value).Returns(Settings);

        LoggerMock = new Mock<ILogger<Infrastructure.Services.AiClientService>>();

        AiClientService = new Infrastructure.Services.AiClientService(
            HttpClient,
            SettingsMock.Object,
            LoggerMock.Object
        );
    }

    /// <summary>
    /// Sets up the mock to return a successful HTTP response with the given content
    /// </summary>
    protected void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        HttpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });
    }

    /// <summary>
    /// Sets up the mock to return a successful AI response
    /// </summary>
    protected void SetupSuccessResponse(AiGenerateResponse response)
    {
        string json = JsonSerializer.Serialize(response, JsonOptions);
        SetupHttpResponse(HttpStatusCode.OK, json);
    }

    /// <summary>
    /// Sets up the mock to throw an HttpRequestException (connection error)
    /// </summary>
    protected void SetupConnectionError()
    {
        HttpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));
    }

    /// <summary>
    /// Sets up the mock to simulate a timeout
    /// </summary>
    protected void SetupTimeout()
    {
        HttpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timed out", new TimeoutException()));
    }

    /// <summary>
    /// Creates a standard test request
    /// </summary>
    protected AiGenerateRequest CreateTestRequest(string query = "Hello AI", int contextCount = 2)
    {
        List<AiContextMessage> context = new();
        for (int i = 0; i < contextCount; i++)
        {
            context.Add(new AiContextMessage
            {
                Id = Guid.NewGuid().ToString(),
                SenderType = "user",
                SenderName = $"User{i}",
                Content = $"Message {i}",
                CreatedAt = DateTime.UtcNow.AddMinutes(-contextCount + i)
            });
        }

        return new AiGenerateRequest
        {
            Provider = "gemini",
            Query = query,
            Context = context,
            Config = new AiGenerateConfig
            {
                Temperature = 0.7m,
                MaxTokens = 2000
            }
        };
    }

    /// <summary>
    /// Creates a standard test response
    /// </summary>
    protected AiGenerateResponse CreateTestResponse(string responseText = "Hello! How can I help you?")
    {
        return new AiGenerateResponse
        {
            Response = responseText,
            Metadata = new AiResponseMetadataDto
            {
                Provider = "gemini",
                Model = "gemini-1.5-pro",
                TokensInput = 100,
                TokensOutput = 50,
                LatencyMs = 250
            },
            Attachment = null
        };
    }

    /// <summary>
    /// Verifies that the HTTP request was sent with the correct method and path
    /// </summary>
    protected void VerifyHttpRequest(HttpMethod expectedMethod, string expectedPath, Times times)
    {
        HttpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                times,
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == expectedMethod &&
                    req.RequestUri!.PathAndQuery == expectedPath),
                ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Verifies that the HTTP request included the API key header
    /// </summary>
    protected void VerifyApiKeyHeader(Times times)
    {
        HttpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                times,
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Headers.Contains("X-API-Key") &&
                    req.Headers.GetValues("X-API-Key").First() == Settings.ApiKey),
                ItExpr.IsAny<CancellationToken>());
    }
}