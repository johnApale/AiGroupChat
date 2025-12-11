# Session 17: AI Client Service Implementation

## Overview

This session implemented the AI Client Service infrastructure that enables the ASP.NET backend to communicate with the Python AI service. When users @mention AI in a message (e.g., `@ai how do I do this?`), the system detects the mention, calls the external AI service, and broadcasts the response back to the group.

## What Was Accomplished

### 1. Configuration

Created settings class and configuration for the AI service connection:

**AiServiceSettings.cs:**

- `BaseUrl` - Python AI service URL (e.g., `http://localhost:8000`)
- `ApiKey` - Service-to-service authentication key
- `TimeoutSeconds` - Request timeout (default: 30)
- `MaxContextMessages` - Max messages to send as context (default: 100)

**appsettings.json updates:**

```json
"AiService": {
  "BaseUrl": "",
  "ApiKey": "",
  "TimeoutSeconds": 30,
  "MaxContextMessages": 100
}
```

### 2. SignalR AI Typing Events

Created new DTOs for AI typing indicators:

| Event                  | Purpose                               |
| ---------------------- | ------------------------------------- |
| `AiTypingEvent`        | Broadcast when AI starts generating   |
| `AiStoppedTypingEvent` | Broadcast when AI finishes generating |

Added corresponding methods to `IChatHubService` and `ChatHubService`:

- `BroadcastAiTypingAsync(groupId, event)`
- `BroadcastAiStoppedTypingAsync(groupId, event)`

### 3. AI Service DTOs

Created DTOs matching the Python AI service API contract:

**Request DTOs (`AiGenerateRequest.cs`):**

- `AiGenerateRequest` - Main request with provider, context, query, config
- `AiContextMessage` - Individual message in conversation context
- `AiGenerateConfig` - Temperature and maxTokens settings

**Response DTOs (`AiGenerateResponse.cs`):**

- `AiGenerateResponse` - Response text, metadata, optional attachment
- `AiResponseMetadataDto` - Provider, model, tokens, latency
- `AiAttachment` - Type, name, base64 content for file attachments

### 4. AI Client Service

**IAiClientService interface:**

```csharp
Task<AiGenerateResponse?> GenerateAsync(AiGenerateRequest request, CancellationToken ct);
```

**AiClientService implementation:**

- Uses typed `HttpClient` with base URL and timeout from settings
- Sends `X-API-Key` header for authentication
- Uses camelCase JSON serialization (matching Python service)
- Returns `null` on any error (timeout, connection failure, bad response)
- Comprehensive logging for debugging

### 5. AI Invocation Service

Separated AI logic into dedicated service for clean architecture:

**IAiInvocationService interface:**

```csharp
bool IsAiMentioned(string content);
Task HandleAsync(Group group, Message triggerMessage, CancellationToken ct);
```

**AiInvocationService implementation:**

- `IsAiMentioned()` - Detects `@ai` at start of message (case-insensitive)
- `StripAiMention()` - Removes `@ai ` prefix from query
- `HandleAsync()` - Full orchestration:
  1. Check if AI is enabled (sends error message if disabled)
  2. Broadcast `AiTyping` event
  3. Fetch AI-visible context messages
  4. Call Python AI service
  5. Save AI response as message
  6. Save response metadata (tokens, cost, latency)
  7. Broadcast AI message to group
  8. Broadcast `AiStoppedTyping` event (always, in finally block)

### 6. AI Response Metadata Repository

Created repository for persisting AI response statistics:

**IAiResponseMetadataRepository:**

```csharp
Task<AiResponseMetadata> CreateAsync(AiResponseMetadata metadata, CancellationToken ct);
```

Saves:

- `Model` - Actual model used (e.g., "gemini-1.5-pro")
- `TokensInput` / `TokensOutput` - Token counts
- `LatencyMs` - Response time
- `CostEstimate` - Calculated from provider's cost settings

### 7. Message Repository Enhancement

Added method for fetching AI context:

```csharp
Task<List<Message>> GetAiContextMessagesAsync(Guid groupId, int maxMessages, CancellationToken ct);
```

Returns AI-visible messages (`AiVisible = true`) in chronological order.

### 8. MessageService Integration

Updated `MessageService.SendMessageAsync` to:

1. Save and broadcast user message (unchanged)
2. Check for AI mention using `_aiInvocationService.IsAiMentioned()`
3. Call `_aiInvocationService.HandleAsync()` if mentioned

The separation keeps `MessageService` focused on message CRUD while `AiInvocationService` handles all AI-specific logic.

## Files Created

```
src/AiGroupChat.Infrastructure/
├── Configuration/
│   └── AiServiceSettings.cs
├── Repositories/
│   └── AiResponseMetadataRepository.cs
└── Services/
    └── AiClientService.cs

src/AiGroupChat.Application/
├── DTOs/
│   └── AiService/
│       ├── AiGenerateRequest.cs
│       └── AiGenerateResponse.cs
├── Interfaces/
│   ├── IAiClientService.cs
│   ├── IAiInvocationService.cs
│   └── IAiResponseMetadataRepository.cs
└── Services/
    └── AiInvocationService.cs

src/AiGroupChat.Application/DTOs/SignalR/GroupChannel/
├── AiTypingEvent.cs
└── AiStoppedTypingEvent.cs
```

## Files Modified

| File                                      | Changes                                                                             |
| ----------------------------------------- | ----------------------------------------------------------------------------------- |
| `appsettings.json`                        | Added `AiService` configuration section                                             |
| `IChatHubService.cs`                      | Added `BroadcastAiTypingAsync`, `BroadcastAiStoppedTypingAsync`                     |
| `ChatHubService.cs`                       | Implemented AI typing broadcast methods                                             |
| `IMessageRepository.cs`                   | Added `GetAiContextMessagesAsync`                                                   |
| `MessageRepository.cs`                    | Implemented AI context message retrieval                                            |
| `MessageService.cs`                       | Refactored to use `IAiInvocationService`                                            |
| `DependencyInjection.cs` (Application)    | Registered `IAiInvocationService`                                                   |
| `DependencyInjection.cs` (Infrastructure) | Registered `AiServiceSettings`, `IAiClientService`, `IAiResponseMetadataRepository` |

## Message Flow

When user sends `@ai how do I do this?`:

```
1. POST /api/groups/:id/messages
   └── MessageService.SendMessageAsync()
       ├── Save user message (AiVisible = group.AiMonitoringEnabled)
       ├── Broadcast MessageReceived (user message)
       ├── Send personal notifications
       └── Detect @ai mention
           └── AiInvocationService.HandleAsync()
               ├── Broadcast AiTyping
               ├── Fetch context (AI-visible messages)
               ├── Call Python AI service
               │   └── POST http://localhost:8000/generate
               │       ├── X-API-Key header
               │       └── { provider, context, query, config }
               ├── Save AI message
               ├── Save AI metadata (tokens, cost, latency)
               ├── Broadcast MessageReceived (AI message)
               └── Broadcast AiStoppedTyping (finally)
```

## Error Handling

| Scenario               | Behavior                                                       |
| ---------------------- | -------------------------------------------------------------- |
| AI disabled for group  | Saves/broadcasts: "AI is currently disabled for this group..." |
| AI service timeout     | Saves/broadcasts: "Sorry, I'm having trouble processing..."    |
| AI service unavailable | Saves/broadcasts: "Sorry, I'm having trouble processing..."    |
| AI service error       | Saves/broadcasts: "Sorry, I'm having trouble processing..."    |

All errors still broadcast `AiStoppedTyping` via `finally` block.

## Configuration Notes

**Development (`appsettings.Development.json`):**

```json
"AiService": {
  "BaseUrl": "http://localhost:8000",
  "ApiKey": "dev-api-key-change-in-production",
  "TimeoutSeconds": 30,
  "MaxContextMessages": 100
}
```

**Production:** Use environment variables for sensitive values:

- `AiService__BaseUrl`
- `AiService__ApiKey`

## Dependencies Added

- Typed `HttpClient` registration for `IAiClientService` with base URL and timeout

## Design Decisions

1. **Separate AiInvocationService** - Keeps AI logic isolated from message CRUD, easier to test and extend.

2. **Null return for errors** - `AiClientService.GenerateAsync()` returns `null` on any failure rather than throwing, letting the caller decide how to handle errors gracefully.

3. **Error messages as AI messages** - Errors are saved as regular AI messages (not separate events), so users see them in chat history and can understand what happened.

4. **Always broadcast AiStoppedTyping** - Using `finally` block ensures typing indicator is cleared even on errors.

5. **Cost calculation** - Automatically calculates cost estimate from provider's `InputTokenCost` and `OutputTokenCost` per 1K tokens.

6. **Context in chronological order** - AI context messages are fetched newest-first then reversed to chronological, giving AI proper conversation flow.

## Next Steps

1. **Unit tests** for `AiInvocationService` and `AiClientService`
2. **Integration tests** for end-to-end AI invocation flow
3. **Python AI service** implementation (separate repo)
4. **AI attachments** - Handle file attachments in AI responses
