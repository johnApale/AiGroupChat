# AI Client Service - Test Implementation Plan

## Overview

This document outlines the testing strategy for the AI Client Service feature, covering unit tests, integration tests, and end-to-end scenarios.

---

## Unit Tests

### 1. AiInvocationService Tests

**Location:** `tests/AiGroupChat.UnitTests/Services/AiInvocationService/`

#### IsAiMentioned Tests (`IsAiMentionedTests.cs`)

| Test                                         | Description               |
| -------------------------------------------- | ------------------------- |
| `WithAiMentionAtStart_ReturnsTrue`           | `@ai how are you` → true  |
| `WithAiMentionUppercase_ReturnsTrue`         | `@AI help me` → true      |
| `WithAiMentionMixedCase_ReturnsTrue`         | `@Ai what is this` → true |
| `WithAiMentionOnly_ReturnsTrue`              | `@ai` → true              |
| `WithAiMentionWithLeadingSpaces_ReturnsTrue` | `  @ai test` → true       |
| `WithAiMentionInMiddle_ReturnsFalse`         | `hello @ai there` → false |
| `WithSimilarPrefix_ReturnsFalse`             | `@aiden hello` → false    |
| `WithNoMention_ReturnsFalse`                 | `hello world` → false     |
| `WithEmptyString_ReturnsFalse`               | `` → false                |

#### HandleAsync Tests - AI Disabled (`HandleAsyncAiDisabledTests.cs`)

| Test                                    | Description                          |
| --------------------------------------- | ------------------------------------ |
| `WhenAiDisabled_SavesDisabledMessage`   | Verify disabled message is saved     |
| `WhenAiDisabled_BroadcastsMessage`      | Verify message is broadcast to group |
| `WhenAiDisabled_DoesNotCallAiService`   | Verify AI service is not called      |
| `WhenAiDisabled_DoesNotBroadcastTyping` | Verify no typing indicators          |

#### HandleAsync Tests - AI Enabled (`HandleAsyncAiEnabledTests.cs`)

| Test                                             | Description                    |
| ------------------------------------------------ | ------------------------------ |
| `WhenAiEnabled_BroadcastsAiTyping`               | Verify AiTyping event is sent  |
| `WhenAiEnabled_FetchesContextMessages`           | Verify context is retrieved    |
| `WhenAiEnabled_CallsAiServiceWithCorrectRequest` | Verify request structure       |
| `WhenAiEnabled_SavesAiResponse`                  | Verify AI message is saved     |
| `WhenAiEnabled_SavesResponseMetadata`            | Verify metadata is saved       |
| `WhenAiEnabled_BroadcastsAiMessage`              | Verify AI message is broadcast |
| `WhenAiEnabled_BroadcastsAiStoppedTyping`        | Verify stopped typing is sent  |
| `WhenAiEnabled_StripsAiMentionFromQuery`         | Verify `@ai ` is removed       |
| `WhenAiMentionOnly_SendsEmptyQuery`              | `@ai` sends empty query        |

#### HandleAsync Tests - Error Scenarios (`HandleAsyncErrorTests.cs`)

| Test                                               | Description                |
| -------------------------------------------------- | -------------------------- |
| `WhenAiServiceReturnsNull_SavesErrorMessage`       | Verify error message saved |
| `WhenAiServiceThrows_SavesErrorMessage`            | Verify exception is caught |
| `WhenAiServiceThrows_StillBroadcastsStoppedTyping` | Verify finally block runs  |
| `WhenAiServiceTimesOut_SavesErrorMessage`          | Verify timeout handling    |

#### Context Building Tests (`BuildAiRequestTests.cs`)

| Test                                | Description                               |
| ----------------------------------- | ----------------------------------------- |
| `BuildsRequestWithCorrectProvider`  | Provider name from group's AI provider    |
| `BuildsRequestWithCorrectConfig`    | Temperature and max tokens from provider  |
| `BuildsContextInChronologicalOrder` | Oldest messages first                     |
| `IncludesUserAndAiMessages`         | Both sender types in context              |
| `UsesDisplayNameForSenderName`      | Falls back to username if no display name |

### 2. AiClientService Tests

**Location:** `tests/AiGroupChat.UnitTests/Services/AiClientService/`

#### GenerateAsync Tests (`GenerateAsyncTests.cs`)

| Test                                              | Description                      |
| ------------------------------------------------- | -------------------------------- |
| `WithValidRequest_SendsCorrectHttpRequest`        | Verify POST to /generate         |
| `WithValidRequest_IncludesApiKeyHeader`           | Verify X-API-Key header          |
| `WithValidRequest_SendsCamelCaseJson`             | Verify JSON serialization        |
| `WithSuccessResponse_ReturnsDeserializedResponse` | Verify response parsing          |
| `WithSuccessResponse_IncludesMetadata`            | Verify metadata is parsed        |
| `WithAttachment_ReturnsAttachmentData`            | Verify attachment parsing        |
| `With400Response_ReturnsNull`                     | Verify error handling            |
| `With500Response_ReturnsNull`                     | Verify error handling            |
| `WithTimeout_ReturnsNull`                         | Verify timeout handling          |
| `WithConnectionError_ReturnsNull`                 | Verify network error handling    |
| `WithInvalidJson_ReturnsNull`                     | Verify JSON parse error handling |

### 3. AiResponseMetadataRepository Tests

**Location:** `tests/AiGroupChat.UnitTests/Repositories/AiResponseMetadataRepository/`

| Test                        | Description                  |
| --------------------------- | ---------------------------- |
| `CreateAsync_SavesMetadata` | Verify metadata is persisted |
| `CreateAsync_SetsAllFields` | Verify all fields are saved  |

### 4. MessageRepository Tests (New Method)

**Location:** `tests/AiGroupChat.UnitTests/Repositories/MessageRepository/`

#### GetAiContextMessagesAsync Tests (`GetAiContextMessagesAsyncTests.cs`)

| Test                                  | Description                         |
| ------------------------------------- | ----------------------------------- |
| `ReturnsOnlyAiVisibleMessages`        | Filter by AiVisible = true          |
| `ReturnsMessagesInChronologicalOrder` | Oldest first                        |
| `RespectsMaxMessagesLimit`            | Returns at most N messages          |
| `IncludesSenderInfo`                  | Includes Sender navigation property |
| `ReturnsEmptyWhenNoMessages`          | No matching messages                |
| `ReturnsLatestMessagesWhenOverLimit`  | Most recent N when > N exist        |

---

## Integration Tests

### 1. AI Invocation Integration Tests

**Location:** `tests/AiGroupChat.IntegrationTests/Messages/AiInvocationTests.cs`

#### Setup

- Mock Python AI service using `WireMock` or similar
- Create test users, groups with AI enabled/disabled

#### Tests

| Test                                                | Description                |
| --------------------------------------------------- | -------------------------- |
| `SendMessage_WithAiMention_TriggersAiResponse`      | Full flow test             |
| `SendMessage_WithAiDisabled_ReturnsDisabledMessage` | AI disabled scenario       |
| `SendMessage_WithAiServiceDown_ReturnsErrorMessage` | Service unavailable        |
| `SendMessage_WithoutAiMention_NoAiResponse`         | Normal message, no AI      |
| `SendMessage_AiResponseHasCorrectSenderType`        | SenderType = "Ai"          |
| `SendMessage_AiResponseHasProviderInfo`             | Uses provider display name |

### 2. SignalR AI Event Tests

**Location:** `tests/AiGroupChat.IntegrationTests/Hubs/ChatHub/AiTypingTests.cs`

| Test                                       | Description                    |
| ------------------------------------------ | ------------------------------ |
| `AiMention_BroadcastsAiTypingEvent`        | Receive AiTyping via WebSocket |
| `AiMention_BroadcastsAiStoppedTypingEvent` | Receive AiStoppedTyping        |
| `AiMention_BroadcastsAiMessage`            | Receive AI MessageReceived     |
| `AiTypingEvent_HasCorrectProviderInfo`     | ProviderId and ProviderName    |
| `OnError_StillBroadcastsStoppedTyping`     | Verify cleanup on error        |

### 3. AI Context Tests

**Location:** `tests/AiGroupChat.IntegrationTests/Messages/AiContextTests.cs`

| Test                                    | Description                            |
| --------------------------------------- | -------------------------------------- |
| `AiOnlySeesAiVisibleMessages`           | Messages with AiVisible=false excluded |
| `AiSeesMessagesInOrder`                 | Chronological context                  |
| `AiContextRespectsCap`                  | Only last N messages                   |
| `ToggleAiMonitoring_AffectsNewMessages` | AiVisible changes with toggle          |

---

## Test Infrastructure Needs

### 1. Mock AI Service

Create a mock HTTP server for integration tests:

```csharp
public class MockAiService : IDisposable
{
    private readonly WireMockServer _server;

    public string BaseUrl => _server.Urls[0];

    public void SetupSuccessResponse(string response, int latencyMs = 100)
    {
        _server.Given(Request.Create().WithPath("/generate"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody(JsonSerializer.Serialize(new AiGenerateResponse
                {
                    Response = response,
                    Metadata = new AiResponseMetadataDto
                    {
                        Provider = "gemini",
                        Model = "gemini-1.5-pro",
                        TokensInput = 100,
                        TokensOutput = 50,
                        LatencyMs = latencyMs
                    }
                })));
    }

    public void SetupErrorResponse(int statusCode)
    {
        _server.Given(Request.Create().WithPath("/generate"))
            .RespondWith(Response.Create().WithStatusCode(statusCode));
    }

    public void SetupTimeout()
    {
        _server.Given(Request.Create().WithPath("/generate"))
            .RespondWith(Response.Create().WithDelay(TimeSpan.FromSeconds(60)));
    }
}
```

### 2. Test Base Classes

**AiInvocationServiceTestBase.cs:**

```csharp
public class AiInvocationServiceTestBase
{
    protected Mock<IMessageRepository> MessageRepositoryMock;
    protected Mock<IAiResponseMetadataRepository> MetadataRepositoryMock;
    protected Mock<IChatHubService> ChatHubServiceMock;
    protected Mock<IAiClientService> AiClientServiceMock;
    protected Mock<ILogger<AiInvocationService>> LoggerMock;
    protected AiInvocationService Service;

    protected Group CreateTestGroup(bool aiEnabled = true);
    protected Message CreateTestMessage(string content);
    protected AiGenerateResponse CreateTestAiResponse(string response);
}
```

**AiClientServiceTestBase.cs:**

```csharp
public class AiClientServiceTestBase
{
    protected Mock<HttpMessageHandler> HttpHandlerMock;
    protected HttpClient HttpClient;
    protected Mock<IOptions<AiServiceSettings>> SettingsMock;
    protected Mock<ILogger<AiClientService>> LoggerMock;
    protected AiClientService Service;

    protected void SetupHttpResponse(HttpStatusCode status, string content);
    protected void SetupHttpTimeout();
}
```

### 3. SignalR Test Helper Updates

Add to `SignalRHelper.cs`:

```csharp
public List<AiTypingEvent> AiTypingEvents { get; } = new();
public List<AiStoppedTypingEvent> AiStoppedTypingEvents { get; } = new();

// In constructor:
_connection.On<AiTypingEvent>("AiTyping", e => AiTypingEvents.Add(e));
_connection.On<AiStoppedTypingEvent>("AiStoppedTyping", e => AiStoppedTypingEvents.Add(e));

// New wait methods:
public Task<AiTypingEvent> WaitForAiTypingAsync(Guid groupId);
public Task<AiStoppedTypingEvent> WaitForAiStoppedTypingAsync(Guid groupId);
```

---

## Test Execution Commands

```bash
# Run all AI-related unit tests
dotnet test tests/AiGroupChat.UnitTests --filter "FullyQualifiedName~AiInvocation or FullyQualifiedName~AiClient"

# Run all AI-related integration tests
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~AiInvocation or FullyQualifiedName~AiTyping or FullyQualifiedName~AiContext"

# Run specific test file
dotnet test --filter "IsAiMentionedTests"
dotnet test --filter "HandleAsyncAiEnabledTests"
dotnet test --filter "AiTypingTests"
```

---

## Test Coverage Goals

| Component                                     | Target Coverage |
| --------------------------------------------- | --------------- |
| `AiInvocationService`                         | 95%             |
| `AiClientService`                             | 90%             |
| `AiResponseMetadataRepository`                | 100%            |
| `MessageRepository.GetAiContextMessagesAsync` | 100%            |

---

## Priority Order

1. **High Priority (implement first):**

   - `IsAiMentionedTests` - Core detection logic
   - `HandleAsyncAiEnabledTests` - Happy path
   - `HandleAsyncErrorTests` - Error handling

2. **Medium Priority:**

   - `GenerateAsyncTests` - HTTP client behavior
   - `AiTypingTests` (integration) - Real-time events
   - `AiContextTests` - Context filtering

3. **Lower Priority:**
   - `BuildAiRequestTests` - Request building details
   - `AiResponseMetadataRepository` tests - Simple CRUD

---

## Dependencies

| Package            | Purpose                               |
| ------------------ | ------------------------------------- |
| `WireMock.Net`     | Mock HTTP server for AI service       |
| `Moq`              | Mocking framework (already installed) |
| `FluentAssertions` | Assertion library (already installed) |

---

## Notes

- Integration tests should use a dedicated test database
- Mock AI service should be started/stopped per test class
- SignalR tests need sequential execution (existing `[Collection("SignalR")]`)
- Consider adding performance tests for AI response handling under load
