# AiClientService Unit Tests

Unit tests for `AiClientService` which handles HTTP communication with the Python AI service.

## Test Files

| File                         | Description                                               |
| ---------------------------- | --------------------------------------------------------- |
| `AiClientServiceTestBase.cs` | Base class with mocked `HttpMessageHandler`, test helpers |
| `GenerateAsyncTests.cs`      | Tests for the `GenerateAsync` method                      |

## Test Coverage

### GenerateAsyncTests

| Test                                              | Description                                 |
| ------------------------------------------------- | ------------------------------------------- |
| `WithValidRequest_SendsCorrectHttpRequest`        | Verifies POST to /generate                  |
| `WithValidRequest_IncludesApiKeyHeader`           | Verifies X-API-Key header                   |
| `WithValidRequest_SendsCamelCaseJson`             | Verifies JSON serialization uses camelCase  |
| `WithSuccessResponse_ReturnsDeserializedResponse` | Verifies response parsing                   |
| `WithSuccessResponse_IncludesMetadata`            | Verifies metadata is parsed correctly       |
| `WithAttachment_ReturnsAttachmentData`            | Verifies attachment parsing                 |
| `With400Response_ReturnsNull`                     | Verifies bad request handling               |
| `With500Response_ReturnsNull`                     | Verifies server error handling              |
| `With401Response_ReturnsNull`                     | Verifies unauthorized handling              |
| `With503Response_ReturnsNull`                     | Verifies service unavailable handling       |
| `WithTimeout_ReturnsNull`                         | Verifies timeout handling                   |
| `WithConnectionError_ReturnsNull`                 | Verifies network error handling             |
| `WithInvalidJson_ReturnsNull`                     | Verifies JSON parse error handling          |
| `WithEmptyResponse_ReturnsNull`                   | Verifies empty response handling            |
| `WithNullJsonResponse_ReturnsNull`                | Verifies null JSON response handling        |
| `WithCancellationToken_PassesTokenToHttpClient`   | Verifies cancellation token forwarding      |
| `WithEmptyContext_SendsRequestSuccessfully`       | Verifies request with no context            |
| `WithLargeContext_SendsRequestSuccessfully`       | Verifies request with many context messages |

## Running Tests

```bash
# Run all AiClientService tests
dotnet test --filter "FullyQualifiedName~AiClientService"

# Run specific test file
dotnet test --filter "GenerateAsyncTests"
```

## Mocking Approach

The tests use `Moq` to mock `HttpMessageHandler` (the underlying handler for `HttpClient`). This is the standard pattern for testing `HttpClient`-based services without making real HTTP calls.

Key helper methods in the base class:

- `SetupHttpResponse(statusCode, content)` - Sets up a mock HTTP response
- `SetupSuccessResponse(response)` - Sets up a successful AI response
- `SetupConnectionError()` - Simulates a connection failure
- `SetupTimeout()` - Simulates a request timeout
- `VerifyHttpRequest(method, path, times)` - Verifies the HTTP request was made correctly
- `VerifyApiKeyHeader(times)` - Verifies the API key header was included
