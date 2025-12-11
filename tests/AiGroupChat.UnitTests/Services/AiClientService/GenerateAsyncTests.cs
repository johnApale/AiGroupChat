using System.Net;
using System.Text.Json;
using AiGroupChat.Application.DTOs.AiService;
using Moq;
using Moq.Protected;

namespace AiGroupChat.UnitTests.Services.AiClientService;

public class GenerateAsyncTests : AiClientServiceTestBase
{
    [Fact]
    public async Task WithValidRequest_SendsCorrectHttpRequest()
    {
        // Arrange
        AiGenerateRequest request = CreateTestRequest();
        AiGenerateResponse expectedResponse = CreateTestResponse();
        SetupSuccessResponse(expectedResponse);

        // Act
        AiGenerateResponse? result = await AiClientService.GenerateAsync(request);

        // Assert
        VerifyHttpRequest(HttpMethod.Post, "/generate", Times.Once());
    }

    [Fact]
    public async Task WithValidRequest_IncludesApiKeyHeader()
    {
        // Arrange
        AiGenerateRequest request = CreateTestRequest();
        AiGenerateResponse expectedResponse = CreateTestResponse();
        SetupSuccessResponse(expectedResponse);

        // Act
        AiGenerateResponse? result = await AiClientService.GenerateAsync(request);

        // Assert
        VerifyApiKeyHeader(Times.Once());
    }

    [Fact]
    public async Task WithValidRequest_SendsCamelCaseJson()
    {
        // Arrange
        AiGenerateRequest request = CreateTestRequest("Test query");
        AiGenerateResponse expectedResponse = CreateTestResponse();
        string? capturedBody = null;

        HttpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>(async (req, ct) =>
            {
                capturedBody = await req.Content!.ReadAsStringAsync(ct);
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse, JsonOptions))
            });

        // Act
        AiGenerateResponse? result = await AiClientService.GenerateAsync(request);

        // Assert
        Assert.NotNull(capturedBody);
        Assert.Contains("\"provider\":", capturedBody); // camelCase
        Assert.Contains("\"query\":", capturedBody);
        Assert.Contains("\"context\":", capturedBody);
        Assert.DoesNotContain("\"Provider\":", capturedBody); // Not PascalCase
        Assert.DoesNotContain("\"Query\":", capturedBody);
    }

    [Fact]
    public async Task WithSuccessResponse_ReturnsDeserializedResponse()
    {
        // Arrange
        AiGenerateRequest request = CreateTestRequest();
        AiGenerateResponse expectedResponse = CreateTestResponse("This is the AI response");
        SetupSuccessResponse(expectedResponse);

        // Act
        AiGenerateResponse? result = await AiClientService.GenerateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Response, result.Response);
    }

    [Fact]
    public async Task WithSuccessResponse_IncludesMetadata()
    {
        // Arrange
        AiGenerateRequest request = CreateTestRequest();
        AiGenerateResponse expectedResponse = new AiGenerateResponse
        {
            Response = "Test response",
            Metadata = new AiResponseMetadataDto
            {
                Provider = "gemini",
                Model = "gemini-1.5-pro",
                TokensInput = 150,
                TokensOutput = 75,
                LatencyMs = 500
            }
        };
        SetupSuccessResponse(expectedResponse);

        // Act
        AiGenerateResponse? result = await AiClientService.GenerateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Metadata);
        Assert.Equal("gemini", result.Metadata.Provider);
        Assert.Equal("gemini-1.5-pro", result.Metadata.Model);
        Assert.Equal(150, result.Metadata.TokensInput);
        Assert.Equal(75, result.Metadata.TokensOutput);
        Assert.Equal(500, result.Metadata.LatencyMs);
    }

    [Fact]
    public async Task WithAttachment_ReturnsAttachmentData()
    {
        // Arrange
        AiGenerateRequest request = CreateTestRequest();
        AiGenerateResponse expectedResponse = new AiGenerateResponse
        {
            Response = "Here's an image",
            Metadata = new AiResponseMetadataDto
            {
                Provider = "gemini",
                Model = "gemini-1.5-pro",
                TokensInput = 100,
                TokensOutput = 50,
                LatencyMs = 300
            },
            Attachment = new AiAttachment
            {
                Type = "image",
                Name = "diagram.png",
                Base64 = "iVBORw0KGgoAAAANSUhEUgAAAAUA"
            }
        };
        SetupSuccessResponse(expectedResponse);

        // Act
        AiGenerateResponse? result = await AiClientService.GenerateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Attachment);
        Assert.Equal("image", result.Attachment.Type);
        Assert.Equal("diagram.png", result.Attachment.Name);
        Assert.Equal("iVBORw0KGgoAAAANSUhEUgAAAAUA", result.Attachment.Base64);
    }

    [Fact]
    public async Task With400Response_ReturnsNull()
    {
        // Arrange
        AiGenerateRequest request = CreateTestRequest();
        SetupHttpResponse(HttpStatusCode.BadRequest, "{\"error\": \"Invalid request\"}");

        // Act
        AiGenerateResponse? result = await AiClientService.GenerateAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task With500Response_ReturnsNull()
    {
        // Arrange
        AiGenerateRequest request = CreateTestRequest();
        SetupHttpResponse(HttpStatusCode.InternalServerError, "{\"error\": \"Internal server error\"}");

        // Act
        AiGenerateResponse? result = await AiClientService.GenerateAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task With401Response_ReturnsNull()
    {
        // Arrange
        AiGenerateRequest request = CreateTestRequest();
        SetupHttpResponse(HttpStatusCode.Unauthorized, "{\"error\": \"Invalid API key\"}");

        // Act
        AiGenerateResponse? result = await AiClientService.GenerateAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task With503Response_ReturnsNull()
    {
        // Arrange
        AiGenerateRequest request = CreateTestRequest();
        SetupHttpResponse(HttpStatusCode.ServiceUnavailable, "{\"error\": \"Service unavailable\"}");

        // Act
        AiGenerateResponse? result = await AiClientService.GenerateAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task WithTimeout_ReturnsNull()
    {
        // Arrange
        AiGenerateRequest request = CreateTestRequest();
        SetupTimeout();

        // Act
        AiGenerateResponse? result = await AiClientService.GenerateAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task WithConnectionError_ReturnsNull()
    {
        // Arrange
        AiGenerateRequest request = CreateTestRequest();
        SetupConnectionError();

        // Act
        AiGenerateResponse? result = await AiClientService.GenerateAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task WithInvalidJson_ReturnsNull()
    {
        // Arrange
        AiGenerateRequest request = CreateTestRequest();
        SetupHttpResponse(HttpStatusCode.OK, "not valid json {{{");

        // Act
        AiGenerateResponse? result = await AiClientService.GenerateAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task WithEmptyResponse_ReturnsNull()
    {
        // Arrange
        AiGenerateRequest request = CreateTestRequest();
        SetupHttpResponse(HttpStatusCode.OK, "");

        // Act
        AiGenerateResponse? result = await AiClientService.GenerateAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task WithNullJsonResponse_ReturnsNull()
    {
        // Arrange
        AiGenerateRequest request = CreateTestRequest();
        SetupHttpResponse(HttpStatusCode.OK, "null");

        // Act
        AiGenerateResponse? result = await AiClientService.GenerateAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task WithCancellationToken_PassesTokenToHttpClient()
    {
        // Arrange
        AiGenerateRequest request = CreateTestRequest();
        CancellationTokenSource cts = new CancellationTokenSource();
        bool tokenWasCancellable = false;

        HttpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) =>
            {
                // Verify the token is linked to our CancellationTokenSource
                // by checking if it can be cancelled
                tokenWasCancellable = ct.CanBeCanceled;
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(CreateTestResponse(), JsonOptions))
            });

        // Act
        AiGenerateResponse? result = await AiClientService.GenerateAsync(request, cts.Token);

        // Assert
        Assert.True(tokenWasCancellable, "CancellationToken should be cancellable");
    }

    [Fact]
    public async Task WithEmptyContext_SendsRequestSuccessfully()
    {
        // Arrange
        AiGenerateRequest request = new AiGenerateRequest
        {
            Provider = "gemini",
            Query = "Hello",
            Context = new List<AiContextMessage>(),
            Config = new AiGenerateConfig { Temperature = 0.7m, MaxTokens = 2000 }
        };
        AiGenerateResponse expectedResponse = CreateTestResponse();
        SetupSuccessResponse(expectedResponse);

        // Act
        AiGenerateResponse? result = await AiClientService.GenerateAsync(request);

        // Assert
        Assert.NotNull(result);
        VerifyHttpRequest(HttpMethod.Post, "/generate", Times.Once());
    }

    [Fact]
    public async Task WithLargeContext_SendsRequestSuccessfully()
    {
        // Arrange
        AiGenerateRequest request = CreateTestRequest("Test", contextCount: 50);
        AiGenerateResponse expectedResponse = CreateTestResponse();
        SetupSuccessResponse(expectedResponse);

        // Act
        AiGenerateResponse? result = await AiClientService.GenerateAsync(request);

        // Assert
        Assert.NotNull(result);
        VerifyHttpRequest(HttpMethod.Post, "/generate", Times.Once());
    }
}