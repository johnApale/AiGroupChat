# AiInvocationService Unit Tests

Unit tests for `AiInvocationService` which handles AI invocation when users @mention AI in messages.

## Test Files

| File                             | Description                                          |
| -------------------------------- | ---------------------------------------------------- |
| `AiInvocationServiceTestBase.cs` | Base class with mocked dependencies, test helpers    |
| `IsAiMentionedTests.cs`          | Tests for AI mention detection                       |
| `HandleAsyncAiDisabledTests.cs`  | Tests for behavior when AI is disabled               |
| `HandleAsyncAiEnabledTests.cs`   | Tests for happy path when AI is enabled              |
| `HandleAsyncErrorTests.cs`       | Tests for error handling scenarios                   |
| `BuildAiRequestTests.cs`         | Tests for AI request building and context formatting |

## Test Coverage

### IsAiMentionedTests (20 tests)

| Test                                                     | Description               |
| -------------------------------------------------------- | ------------------------- |
| `WithAiMentionAtStart_ReturnsTrue`                       | `@ai how are you` → true  |
| `WithAiMentionUppercase_ReturnsTrue`                     | `@AI help me` → true      |
| `WithAiMentionMixedCase_ReturnsTrue`                     | `@Ai what is this` → true |
| `WithAiMentionOnly_ReturnsTrue`                          | `@ai` → true              |
| `WithAiMentionOnlyUppercase_ReturnsTrue`                 | `@AI` → true              |
| `WithAiMentionWithLeadingSpaces_ReturnsTrue`             | `  @ai test` → true       |
| `WithAiMentionWithLeadingTabs_ReturnsTrue`               | `\t@ai test` → true       |
| `WithAiMentionWithMultipleLeadingWhitespace_ReturnsTrue` | Mixed whitespace → true   |
| `WithAiMentionInMiddle_ReturnsFalse`                     | `hello @ai there` → false |
| `WithAiMentionAtEnd_ReturnsFalse`                        | `hello @ai` → false       |
| `WithSimilarPrefix_ReturnsFalse`                         | `@aiden hello` → false    |
| `WithAiWithoutSpace_ReturnsFalse`                        | `@aihelp` → false         |
| `WithNoMention_ReturnsFalse`                             | `hello world` → false     |
| `WithEmptyString_ReturnsFalse`                           | `` → false                |
| `WithWhitespaceOnly_ReturnsFalse`                        | `   ` → false             |
| `WithAtSymbolOnly_ReturnsFalse`                          | `@` → false               |
| `WithPartialAiMention_ReturnsFalse`                      | `@a` → false              |
| `WithDifferentMention_ReturnsFalse`                      | `@bob hello` → false      |
| `WithAiInText_ReturnsFalse`                              | `ai is great` → false     |

### HandleAsyncAiDisabledTests (8 tests)

| Test                                           | Description                          |
| ---------------------------------------------- | ------------------------------------ |
| `WhenAiDisabled_SavesDisabledMessage`          | Verify disabled message is saved     |
| `WhenAiDisabled_BroadcastsMessage`             | Verify message is broadcast to group |
| `WhenAiDisabled_DoesNotCallAiService`          | Verify AI service is not called      |
| `WhenAiDisabled_DoesNotBroadcastTyping`        | Verify no typing indicators          |
| `WhenAiDisabled_DoesNotBroadcastStoppedTyping` | Verify no stopped typing             |
| `WhenAiDisabled_DoesNotFetchContextMessages`   | Verify context not fetched           |
| `WhenAiDisabled_DoesNotSaveMetadata`           | Verify no metadata saved             |
| `WhenAiDisabled_MessageHasCorrectAiProviderId` | Provider ID on message               |
| `WhenAiDisabled_MessageIsAiVisible`            | AiVisible flag is true               |

### HandleAsyncAiEnabledTests (13 tests)

| Test                                              | Description                    |
| ------------------------------------------------- | ------------------------------ |
| `WhenAiEnabled_BroadcastsAiTyping`                | Verify AiTyping event is sent  |
| `WhenAiEnabled_AiTypingHasCorrectProviderInfo`    | Provider info in typing event  |
| `WhenAiEnabled_FetchesContextMessages`            | Verify context is retrieved    |
| `WhenAiEnabled_CallsAiServiceWithCorrectRequest`  | Verify request structure       |
| `WhenAiEnabled_SavesAiResponse`                   | Verify AI message is saved     |
| `WhenAiEnabled_SavesResponseMetadata`             | Verify metadata is saved       |
| `WhenAiEnabled_BroadcastsAiMessage`               | Verify AI message is broadcast |
| `WhenAiEnabled_BroadcastsAiStoppedTyping`         | Verify stopped typing is sent  |
| `WhenAiEnabled_StripsAiMentionFromQuery`          | Verify `@ai ` is removed       |
| `WhenAiMentionOnly_SendsEmptyQuery`               | `@ai` sends empty query        |
| `WhenAiEnabled_AiResponseHasProviderAsSenderName` | Provider name in response      |
| `WhenAiEnabled_CalculatesCostEstimate`            | Cost calculation from tokens   |

### HandleAsyncErrorTests (12 tests)

| Test                                                         | Description                  |
| ------------------------------------------------------------ | ---------------------------- |
| `WhenAiServiceReturnsNull_SavesErrorMessage`                 | Verify error message saved   |
| `WhenAiServiceReturnsNull_DoesNotSaveMetadata`               | No metadata on null response |
| `WhenAiServiceReturnsNull_StillBroadcastsStoppedTyping`      | Finally block runs           |
| `WhenAiServiceThrows_SavesErrorMessage`                      | Exception caught             |
| `WhenAiServiceThrows_StillBroadcastsStoppedTyping`           | Finally block runs           |
| `WhenAiServiceThrowsHttpRequestException_SavesErrorMessage`  | Network error handling       |
| `WhenAiServiceThrowsTaskCanceledException_SavesErrorMessage` | Timeout handling             |
| `WhenAiServiceThrows_DoesNotSaveMetadata`                    | No metadata on exception     |
| `WhenAiServiceThrows_BroadcastsAiTypingBeforeError`          | Typing sent before error     |
| `WhenAiServiceThrows_BroadcastsErrorMessage`                 | Error message broadcast      |
| `WhenContextFetchFails_SavesErrorMessage`                    | Database error handling      |
| `WhenContextFetchFails_StillBroadcastsStoppedTyping`         | Finally block runs           |
| `WhenErrorOccurs_AiClientNotCalled`                          | AI not called on early error |

### BuildAiRequestTests (12 tests)

| Test                                   | Description                              |
| -------------------------------------- | ---------------------------------------- |
| `BuildsRequestWithCorrectProvider`     | Provider name from group's AI provider   |
| `BuildsRequestWithCorrectConfig`       | Temperature and max tokens from provider |
| `BuildsContextInChronologicalOrder`    | Oldest messages first                    |
| `IncludesUserAndAiMessages`            | Both sender types in context             |
| `UsesDisplayNameForSenderName`         | DisplayName preferred                    |
| `WithEmptyDisplayName_UsesEmptyString` | Empty DisplayName behavior               |
| `FallsBackToUnknownIfNoSender`         | "Unknown" if no sender                   |
| `IncludesMessageIdInContext`           | Message ID in context                    |
| `IncludesCreatedAtInContext`           | Timestamp in context                     |
| `WithEmptyContext_StillCallsAiService` | Empty context handled                    |
| `SenderTypeLowercase`                  | SenderType is lowercase                  |

## Running Tests

```bash
# Run all AiInvocationService tests
dotnet test --filter "FullyQualifiedName~AiInvocationService"

# Run specific test file
dotnet test --filter "IsAiMentionedTests"
dotnet test --filter "HandleAsyncAiEnabledTests"
dotnet test --filter "HandleAsyncAiDisabledTests"
dotnet test --filter "HandleAsyncErrorTests"
dotnet test --filter "BuildAiRequestTests"
```

## Key Testing Patterns

### Mocked Dependencies

- `IMessageRepository` - Message storage and context retrieval
- `IAiResponseMetadataRepository` - AI response metadata storage
- `IChatHubService` - SignalR event broadcasting
- `IAiClientService` - Python AI service communication
- `ILogger<AiInvocationService>` - Logging

### Helper Methods in Base Class

- `CreateTestAiProvider()` - Creates a configured AI provider
- `CreateTestGroup()` - Creates a group with AI settings
- `CreateTriggerMessage()` - Creates a user message with @ai mention
- `CreateContextMessages()` - Creates a list of context messages
- `CreateTestAiResponse()` - Creates a successful AI response
- `SetupContextMessages()` - Configures repository to return context
- `SetupAiClientSuccess()` / `SetupAiClientFailure()` - Configure AI service responses
- `VerifyAiTypingBroadcast()` / `VerifyAiStoppedTypingBroadcast()` - Verify SignalR events
- `VerifyMessageSaved()` / `VerifyMetadataSaved()` - Verify repository calls
