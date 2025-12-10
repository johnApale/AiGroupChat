# SignalR Testing Implementation Plan

## Overview

This plan covers full test coverage for SignalR functionality in the AI Group Chat application. It includes:

1. **Unit Tests** - Test individual components in isolation using mocks
2. **Integration Tests** - Test real WebSocket connections end-to-end

---

## Current State Analysis

### What Exists

**Unit Tests (Partial)**

- `SendMessageBroadcastTests.cs` - Tests `MessageService` calls `BroadcastMessageAsync`
- `UpdateAiSettingsBroadcastTests.cs` - Tests `GroupService` calls `BroadcastAiSettingsChangedAsync`
- `MemberBroadcastTests.cs` - Tests `GroupMemberService` calls member broadcast methods
- `PersonalChannelNotificationTests.cs` - Tests personal channel notifications
- `ConnectionTrackerTests.cs` - Tests the `ConnectionTracker` service

**What's Missing**

- Unit tests for `ChatHub` class (client→server methods)
- Unit tests for `ChatHubService` class (server→client broadcasts)
- Integration tests for WebSocket connections
- Integration tests for real-time message delivery
- Integration tests for presence (online/offline)

### Components to Test

| Component           | Type      | Location     | Purpose                                       |
| ------------------- | --------- | ------------ | --------------------------------------------- |
| `ChatHub`           | Hub       | API/Hubs     | Handles client connections, JoinGroup, typing |
| `ChatHubService`    | Service   | API/Services | Broadcasts events via IHubContext             |
| `ConnectionTracker` | Service   | API/Services | Tracks user connections for presence          |
| `IChatHubService`   | Interface | Application  | Abstraction for broadcasting                  |

---

## Phase 1: Unit Tests for ChatHub

### 1.1 File Structure

```
tests/AiGroupChat.UnitTests/
└── Hubs/
    └── ChatHub/
        ├── ChatHubTestBase.cs          # Base class with mocks
        ├── JoinGroupTests.cs           # JoinGroup method tests
        ├── LeaveGroupTests.cs          # LeaveGroup method tests
        ├── StartTypingTests.cs         # StartTyping method tests
        ├── StopTypingTests.cs          # StopTyping method tests
        ├── OnConnectedAsyncTests.cs    # Connection lifecycle tests
        ├── OnDisconnectedAsyncTests.cs # Disconnection lifecycle tests
        └── README.md                   # Test documentation
```

### 1.2 Test Cases

#### JoinGroupTests (4 tests)

| Test Name                                            | Scenario                        |
| ---------------------------------------------------- | ------------------------------- |
| `JoinGroup_WhenUserIsMember_AddsToSignalRGroup`      | Valid member joins successfully |
| `JoinGroup_WhenUserIsNotMember_ThrowsHubException`   | Non-member gets rejected        |
| `JoinGroup_WhenUserIsOwner_AddsToSignalRGroup`       | Owner can join their group      |
| `JoinGroup_WhenGroupDoesNotExist_ThrowsHubException` | Invalid group ID rejected       |

#### LeaveGroupTests (2 tests)

| Test Name                            | Scenario                                    |
| ------------------------------------ | ------------------------------------------- |
| `LeaveGroup_RemovesFromSignalRGroup` | User successfully leaves                    |
| `LeaveGroup_WhenNotInGroup_NoError`  | Gracefully handles leaving non-joined group |

#### StartTypingTests (4 tests)

| Test Name                                    | Scenario                                |
| -------------------------------------------- | --------------------------------------- |
| `StartTyping_WhenMember_BroadcastsToOthers`  | Broadcasts UserTyping event             |
| `StartTyping_WhenNotMember_DoesNotBroadcast` | Silently ignores non-members            |
| `StartTyping_BroadcastsCorrectUserInfo`      | Event contains correct user data        |
| `StartTyping_ExcludesSender`                 | Sender doesn't receive their own typing |

#### StopTypingTests (3 tests)

| Test Name                                   | Scenario                              |
| ------------------------------------------- | ------------------------------------- |
| `StopTyping_WhenMember_BroadcastsToOthers`  | Broadcasts UserStoppedTyping event    |
| `StopTyping_WhenNotMember_DoesNotBroadcast` | Silently ignores non-members          |
| `StopTyping_BroadcastsCorrectData`          | Event contains correct groupId/userId |

#### OnConnectedAsyncTests (5 tests)

| Test Name                                                  | Scenario                              |
| ---------------------------------------------------------- | ------------------------------------- |
| `OnConnectedAsync_AddsToPersonalChannel`                   | Auto-joins user-{userId} group        |
| `OnConnectedAsync_TracksConnection`                        | Calls ConnectionTracker.AddConnection |
| `OnConnectedAsync_FirstConnection_BroadcastsUserOnline`    | Presence broadcast on first connect   |
| `OnConnectedAsync_SecondConnection_DoesNotBroadcastOnline` | No broadcast for additional tabs      |
| `OnConnectedAsync_BroadcastsToSharedGroupUsers`            | Online event goes to correct users    |

#### OnDisconnectedAsyncTests (5 tests)

| Test Name                                                  | Scenario                                 |
| ---------------------------------------------------------- | ---------------------------------------- |
| `OnDisconnectedAsync_RemovesConnection`                    | Calls ConnectionTracker.RemoveConnection |
| `OnDisconnectedAsync_LastConnection_BroadcastsUserOffline` | Presence broadcast on last disconnect    |
| `OnDisconnectedAsync_NotLastConnection_DoesNotBroadcast`   | No broadcast if other tabs open          |
| `OnDisconnectedAsync_BroadcastsToSharedGroupUsers`         | Offline event goes to correct users      |
| `OnDisconnectedAsync_WithException_StillCleansUp`          | Handles disconnect errors gracefully     |

### 1.3 Mocking Strategy for Hub Tests

```csharp
// Required mocks for ChatHub tests
Mock<IGroupRepository> _groupRepositoryMock;
Mock<IUserRepository> _userRepositoryMock;
Mock<IGroupMemberRepository> _groupMemberRepositoryMock;
Mock<IConnectionTracker> _connectionTrackerMock;
Mock<IChatHubService> _chatHubServiceMock;
Mock<ILogger<ChatHub>> _loggerMock;

// SignalR-specific mocks
Mock<IHubCallerClients> _clientsMock;
Mock<IGroupManager> _groupsMock;
Mock<HubCallerContext> _contextMock;
Mock<IClientProxy> _clientProxyMock;
Mock<IClientProxy> _othersInGroupProxyMock;
```

---

## Phase 2: Unit Tests for ChatHubService

### 2.1 File Structure

```
tests/AiGroupChat.UnitTests/
└── Services/
    └── ChatHubService/
        ├── ChatHubServiceTestBase.cs       # Base class with IHubContext mock
        ├── BroadcastMessageTests.cs        # Message broadcasting
        ├── BroadcastAiSettingsTests.cs     # AI settings broadcasting
        ├── BroadcastMemberEventsTests.cs   # Member join/leave/role
        ├── BroadcastTypingTests.cs         # Typing indicators
        ├── PersonalChannelTests.cs         # Personal notifications
        ├── PresenceTests.cs                # Online/offline broadcasts
        └── README.md                       # Test documentation
```

### 2.2 Test Cases

#### BroadcastMessageTests (3 tests)

| Test Name                                     | Scenario                            |
| --------------------------------------------- | ----------------------------------- |
| `BroadcastMessageAsync_SendsToCorrectGroup`   | Message goes to group-{groupId}     |
| `BroadcastMessageAsync_SendsCorrectEventName` | Event name is "MessageReceived"     |
| `BroadcastMessageAsync_SendsCorrectPayload`   | MessageResponse is passed correctly |

#### BroadcastAiSettingsTests (3 tests)

| Test Name                                               | Scenario                       |
| ------------------------------------------------------- | ------------------------------ |
| `BroadcastAiSettingsChangedAsync_SendsToCorrectGroup`   | Settings go to group-{groupId} |
| `BroadcastAiSettingsChangedAsync_SendsCorrectEventName` | Event is "AiSettingsChanged"   |
| `BroadcastAiSettingsChangedAsync_SendsCorrectPayload`   | AiSettingsChangedEvent passed  |

#### BroadcastMemberEventsTests (6 tests)

| Test Name                                          | Scenario                          |
| -------------------------------------------------- | --------------------------------- |
| `BroadcastMemberJoinedAsync_SendsToGroup`          | MemberJoined event broadcast      |
| `BroadcastMemberLeftAsync_SendsToGroup`            | MemberLeft event broadcast        |
| `BroadcastMemberRoleChangedAsync_SendsToGroup`     | MemberRoleChanged event broadcast |
| `BroadcastMemberJoinedAsync_CorrectEventName`      | Event name verification           |
| `BroadcastMemberLeftAsync_CorrectEventName`        | Event name verification           |
| `BroadcastMemberRoleChangedAsync_CorrectEventName` | Event name verification           |

#### BroadcastTypingTests (4 tests)

| Test Name                                        | Scenario                   |
| ------------------------------------------------ | -------------------------- |
| `BroadcastUserTypingAsync_SendsToGroup`          | UserTyping to group        |
| `BroadcastUserStoppedTypingAsync_SendsToGroup`   | UserStoppedTyping to group |
| `BroadcastUserTypingAsync_CorrectPayload`        | Typing event data          |
| `BroadcastUserStoppedTypingAsync_CorrectPayload` | Stopped typing event data  |

#### PersonalChannelTests (10 tests)

| Test Name                                                | Scenario                  |
| -------------------------------------------------------- | ------------------------- |
| `SendGroupActivityAsync_SendsToPersonalChannel`          | Activity to user-{userId} |
| `SendNewMessageNotificationAsync_SendsToPersonalChannel` | Notification to user      |
| `SendAddedToGroupAsync_SendsToPersonalChannel`           | Added notification        |
| `SendRemovedFromGroupAsync_SendsToPersonalChannel`       | Removed notification      |
| `SendRoleChangedAsync_SendsToPersonalChannel`            | Role change notification  |
| `SendGroupActivityAsync_CorrectEventName`                | "GroupActivity" event     |
| `SendNewMessageNotificationAsync_CorrectEventName`       | "NewMessageNotification"  |
| `SendAddedToGroupAsync_CorrectEventName`                 | "AddedToGroup" event      |
| `SendRemovedFromGroupAsync_CorrectEventName`             | "RemovedFromGroup" event  |
| `SendRoleChangedAsync_CorrectEventName`                  | "RoleChanged" event       |

#### PresenceTests (6 tests)

| Test Name                                    | Scenario                    |
| -------------------------------------------- | --------------------------- |
| `SendUserOnlineAsync_SendsToMultipleUsers`   | Online to all shared users  |
| `SendUserOfflineAsync_SendsToMultipleUsers`  | Offline to all shared users |
| `SendUserOnlineAsync_EmptyList_DoesNotSend`  | Handles no shared users     |
| `SendUserOfflineAsync_EmptyList_DoesNotSend` | Handles no shared users     |
| `SendUserOnlineAsync_CorrectEventName`       | "UserOnline" event          |
| `SendUserOfflineAsync_CorrectEventName`      | "UserOffline" event         |

---

## Phase 3: Integration Tests for SignalR

### 3.1 File Structure

```
tests/AiGroupChat.IntegrationTests/
├── Infrastructure/
│   ├── CustomWebApplicationFactory.cs    # (no changes needed)
│   ├── IntegrationTestBase.cs            # (no changes needed)
│   ├── SignalRIntegrationTestBase.cs     # NEW: Base class for SignalR tests
│   └── SignalRCollection.cs              # NEW: xUnit collection for sequential execution
├── Helpers/
│   └── SignalRHelper.cs                  # NEW: Helper for SignalR operations
└── Hubs/
    └── ChatHub/
        ├── ConnectionTests.cs            # Connection/auth tests
        ├── JoinLeaveGroupTests.cs        # Group subscription tests
        ├── TypingIndicatorTests.cs       # Typing broadcast tests
        ├── MessageBroadcastTests.cs      # REST→WebSocket message flow
        ├── MemberEventTests.cs           # Member changes → broadcasts
        ├── AiSettingsEventTests.cs       # AI settings → broadcasts
        ├── PresenceTests.cs              # Online/offline broadcasts
        └── README.md                     # Test documentation
```

### 3.2 Infrastructure Changes

#### Add SignalR Client Package

```xml
<!-- AiGroupChat.IntegrationTests.csproj -->
<PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="9.0.0" />
```

#### New Files to Create

1. **SignalRHelper.cs** - Manages a single SignalR connection with event collection
2. **SignalRIntegrationTestBase.cs** - Base class with factory method for creating connections
3. **SignalRCollection.cs** - xUnit collection definition for sequential execution

See [Design Decisions](#design-decisions) section for implementation details.

### 3.3 Integration Test Cases

#### ConnectionTests (6 tests)

| Test Name                          | Scenario                         |
| ---------------------------------- | -------------------------------- |
| `Connect_WithValidToken_Succeeds`  | Authenticated connection works   |
| `Connect_WithExpiredToken_Fails`   | Expired JWT rejected             |
| `Connect_WithInvalidToken_Fails`   | Malformed JWT rejected           |
| `Connect_WithoutToken_Fails`       | Missing auth rejected            |
| `Connect_AutoJoinsPersonalChannel` | Personal channel auto-subscribed |
| `Reconnect_RejoinsPreviousGroups`  | Reconnection handling            |

#### JoinLeaveGroupTests (6 tests)

| Test Name                                 | Scenario                         |
| ----------------------------------------- | -------------------------------- |
| `JoinGroup_WhenMember_Succeeds`           | Member can join                  |
| `JoinGroup_WhenNotMember_ThrowsException` | Non-member rejected              |
| `JoinGroup_WhenOwner_Succeeds`            | Owner can join                   |
| `LeaveGroup_Succeeds`                     | User can leave                   |
| `JoinGroup_ReceivesSubsequentMessages`    | Joined user gets messages        |
| `LeaveGroup_StopsReceivingMessages`       | Left user stops getting messages |

#### TypingIndicatorTests (5 tests)

| Test Name                              | Scenario                 |
| -------------------------------------- | ------------------------ |
| `StartTyping_OtherMembersReceiveEvent` | Typing broadcast works   |
| `StartTyping_SenderDoesNotReceive`     | Sender excluded          |
| `StopTyping_OtherMembersReceiveEvent`  | Stop typing broadcast    |
| `StartTyping_NonMember_NoEvent`        | Non-member can't trigger |
| `TypingEvent_ContainsCorrectUserInfo`  | Event payload correct    |

#### MessageBroadcastTests (7 tests)

| Test Name                                      | Scenario                  |
| ---------------------------------------------- | ------------------------- |
| `SendMessage_JoinedMembersReceiveViaWebSocket` | REST→WS flow works        |
| `SendMessage_NotJoinedMembers_DoNotReceive`    | Non-joined don't get it   |
| `SendMessage_MessageContainsAllFields`         | Full payload received     |
| `SendMessage_MultipleJoinedUsers_AllReceive`   | Multi-user broadcast      |
| `SendMessage_SenderAlsoReceives`               | Sender gets their message |
| `SendMessage_NonGroupMembers_DoNotReceive`     | Isolated to group         |
| `SendMessage_AiMessage_BroadcastsCorrectly`    | AI messages work too      |

#### MemberEventTests (8 tests)

| Test Name                                            | Scenario                  |
| ---------------------------------------------------- | ------------------------- |
| `AddMember_JoinedMembersReceiveMemberJoined`         | Add triggers broadcast    |
| `RemoveMember_JoinedMembersReceiveMemberLeft`        | Remove triggers broadcast |
| `UpdateRole_JoinedMembersReceiveMemberRoleChanged`   | Role change broadcast     |
| `AddMember_NewMemberReceivesAddedToGroup`            | Personal notification     |
| `RemoveMember_RemovedMemberReceivesRemovedFromGroup` | Personal notification     |
| `UpdateRole_AffectedMemberReceivesRoleChanged`       | Personal notification     |
| `LeaveGroup_MembersReceiveMemberLeft`                | Self-leave broadcast      |
| `TransferOwnership_BroadcastsRoleChanges`            | Ownership transfer events |

#### AiSettingsEventTests (4 tests)

| Test Name                                               | Scenario              |
| ------------------------------------------------------- | --------------------- |
| `UpdateAiSettings_JoinedMembersReceiveEvent`            | AI toggle broadcasts  |
| `UpdateAiSettings_EventContainsAllFields`               | Full payload received |
| `UpdateAiSettings_ChangeProvider_BroadcastsNewProvider` | Provider change       |
| `UpdateAiSettings_NonJoinedMembers_DoNotReceive`        | Only joined receive   |

#### PresenceTests (8 tests)

| Test Name                                             | Scenario             |
| ----------------------------------------------------- | -------------------- |
| `Connect_FirstConnection_SharedUsersReceiveOnline`    | Online broadcast     |
| `Connect_SecondConnection_NoOnlineBroadcast`          | Multi-tab no spam    |
| `Disconnect_LastConnection_SharedUsersReceiveOffline` | Offline broadcast    |
| `Disconnect_NotLastConnection_NoOfflineBroadcast`     | Multi-tab disconnect |
| `OnlineEvent_ContainsCorrectUserInfo`                 | Payload verification |
| `OfflineEvent_ContainsCorrectUserInfo`                | Payload verification |
| `OnlineEvent_OnlyToSharedGroupMembers`                | Correct audience     |
| `OfflineEvent_OnlyToSharedGroupMembers`               | Correct audience     |

---

## Phase 4: Test Implementation Order

### Step 1: Setup Infrastructure (1-2 hours)

1. Add `Microsoft.AspNetCore.SignalR.Client` to integration tests project
2. Create `SignalRCollection.cs` (xUnit collection definition)
3. Create `SignalRHelper.cs` with connection management and event collection
4. Create `SignalRIntegrationTestBase.cs` with factory method
5. Verify a simple connection test works

### Step 2: ChatHub Unit Tests (3-4 hours)

1. Create `ChatHubTestBase.cs` with all mocks
2. Implement `JoinGroupTests.cs` (4 tests)
3. Implement `LeaveGroupTests.cs` (2 tests)
4. Implement `StartTypingTests.cs` (4 tests)
5. Implement `StopTypingTests.cs` (3 tests)
6. Implement `OnConnectedAsyncTests.cs` (5 tests)
7. Implement `OnDisconnectedAsyncTests.cs` (5 tests)
8. Create `README.md`

### Step 3: ChatHubService Unit Tests (2-3 hours)

1. Create `ChatHubServiceTestBase.cs` with IHubContext mock
2. Implement `BroadcastMessageTests.cs` (3 tests)
3. Implement `BroadcastAiSettingsTests.cs` (3 tests)
4. Implement `BroadcastMemberEventsTests.cs` (6 tests)
5. Implement `BroadcastTypingTests.cs` (4 tests)
6. Implement `PersonalChannelTests.cs` (10 tests)
7. Implement `PresenceTests.cs` (6 tests)
8. Create `README.md`

### Step 4: Integration Tests - Connection (1-2 hours)

1. Implement `ConnectionTests.cs` (6 tests)
2. Verify JWT authentication works via WebSocket

### Step 5: Integration Tests - Core Functionality (3-4 hours)

1. Implement `JoinLeaveGroupTests.cs` (6 tests)
2. Implement `TypingIndicatorTests.cs` (5 tests)
3. Implement `MessageBroadcastTests.cs` (7 tests)

### Step 6: Integration Tests - Events (2-3 hours)

1. Implement `MemberEventTests.cs` (8 tests)
2. Implement `AiSettingsEventTests.cs` (4 tests)

### Step 7: Integration Tests - Presence (2 hours)

1. Implement `PresenceTests.cs` (8 tests)

### Step 8: Documentation (1 hour)

1. Create `README.md` for integration hub tests
2. Update main integration tests README
3. Update `signalr-implementation.md` with test coverage section

---

## Test Count Summary

| Category                 | File                       | Tests        |
| ------------------------ | -------------------------- | ------------ |
| **Unit: ChatHub**        | JoinGroupTests             | 4            |
|                          | LeaveGroupTests            | 2            |
|                          | StartTypingTests           | 4            |
|                          | StopTypingTests            | 3            |
|                          | OnConnectedAsyncTests      | 5            |
|                          | OnDisconnectedAsyncTests   | 5            |
| **Unit: ChatHubService** | BroadcastMessageTests      | 3            |
|                          | BroadcastAiSettingsTests   | 3            |
|                          | BroadcastMemberEventsTests | 6            |
|                          | BroadcastTypingTests       | 4            |
|                          | PersonalChannelTests       | 10           |
|                          | PresenceTests              | 6            |
| **Integration: ChatHub** | ConnectionTests            | 6            |
|                          | JoinLeaveGroupTests        | 6            |
|                          | TypingIndicatorTests       | 5            |
|                          | MessageBroadcastTests      | 7            |
|                          | MemberEventTests           | 8            |
|                          | AiSettingsEventTests       | 4            |
|                          | PresenceTests              | 8            |
| **Total**                |                            | **99 tests** |

---

## Dependencies & Prerequisites

### NuGet Packages Required

```xml
<!-- Unit Tests (already have Moq) -->
<PackageReference Include="Moq" Version="4.20.72" />

<!-- Integration Tests (add SignalR client) -->
<PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="9.0.0" />
```

### Potential Challenges & Mitigations

| Challenge                                                                                         | Mitigation                                                                            |
| ------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------- |
| **Hub Context Mocking** - SignalR's `IHubCallerClients` and `IGroupManager` require careful setup | Create reusable mock factories in `ChatHubTestBase`                                   |
| **Async Event Collection** - Integration tests need to wait for WebSocket events                  | Use `TaskCompletionSource` with 5-second timeouts in `SignalRHelper.WaitFor*` methods |
| **Test Isolation** - SignalR connections must be properly disposed between tests                  | `SignalRIntegrationTestBase` tracks and auto-disposes all connections                 |
| **Timing Issues** - Race conditions when testing real-time events                                 | Event collectors + `WaitFor*` methods with predicates ensure we catch events          |
| **Parallel Execution** - Shared state could cause flaky tests                                     | `[Collection("SignalR")]` ensures sequential execution                                |

---

## Design Decisions

### 1. Timeout Duration

**Decision**: 5 seconds for waiting on WebSocket events.

This provides enough buffer for slower CI environments while failing fast enough to not slow down the test suite significantly.

### 2. Multi-User Test Approach

**Decision**: Factory method on base class with automatic cleanup tracking.

```csharp
// Base class provides factory method
protected async Task<SignalRHelper> CreateSignalRConnectionAsync(string accessToken)
{
    SignalRHelper connection = new SignalRHelper(_factory.Server.BaseAddress);
    await connection.ConnectAsync(accessToken);
    _signalRConnections.Add(connection); // Track for automatic disposal
    return connection;
}

// Usage in tests
SignalRHelper userAConnection = await CreateSignalRConnectionAsync(userAToken);
SignalRHelper userBConnection = await CreateSignalRConnectionAsync(userBToken);

await userAConnection.JoinGroupAsync(groupId);
await userBConnection.JoinGroupAsync(groupId);

// Send message via REST, verify userB receives via WebSocket
await Messages.SendMessageAsync(groupId, "Hello!");
await userBConnection.WaitForMessageAsync(m => m.Content == "Hello!");
```

**Rationale**:

- Each `SignalRHelper` instance manages one connection (simple, explicit)
- Factory method tracks all connections for automatic disposal in `DisposeAsync`
- Tests are self-contained and easy to reason about
- No complex state management inside the helper

### 3. Test Naming Convention

**Decision**: Follow existing pattern `MethodName_Condition_ExpectedResult`

Examples:

- `JoinGroup_WhenUserIsMember_AddsToSignalRGroup`
- `Connect_WithExpiredToken_Fails`
- `SendMessage_JoinedMembersReceiveViaWebSocket`

### 4. Parallel Execution

**Decision**: Sequential execution for SignalR integration tests using xUnit collections.

```csharp
[Collection("SignalR")]
public class ConnectionTests : SignalRIntegrationTestBase
{
    // Tests in this class run sequentially
}

[Collection("SignalR")]
public class MessageBroadcastTests : SignalRIntegrationTestBase
{
    // Also runs sequentially with ConnectionTests
}
```

**Rationale**:

- `ConnectionTracker` is a singleton - parallel tests could interfere with presence state
- Database operations in parallel can cause race conditions
- Sequential execution is more stable and easier to debug
- The 44 integration tests will still complete quickly since each test is fast

### 5. SignalRHelper Design

**Decision**: Simple, single-connection-per-instance design.

```csharp
public class SignalRHelper : IAsyncDisposable
{
    private HubConnection? _connection;

    // Event collectors (populated by event handlers)
    public List<MessageResponse> ReceivedMessages { get; } = new();
    public List<UserTypingEvent> TypingEvents { get; } = new();
    // ... more collectors

    // Lifecycle
    public Task ConnectAsync(string accessToken);
    public ValueTask DisposeAsync();

    // Hub method invocations
    public Task JoinGroupAsync(Guid groupId);
    public Task LeaveGroupAsync(Guid groupId);
    public Task StartTypingAsync(Guid groupId);
    public Task StopTypingAsync(Guid groupId);

    // Waiting utilities with 5-second default timeout
    public Task<MessageResponse> WaitForMessageAsync(Func<MessageResponse, bool> predicate);
    public Task<UserTypingEvent> WaitForTypingEventAsync(Func<UserTypingEvent, bool> predicate);
    // ... more wait methods

    // Utilities
    public void ClearEvents();
}
```

---

## Test Infrastructure Classes

### SignalRIntegrationTestBase

A new base class specifically for SignalR integration tests:

```csharp
[Collection("SignalR")]
public abstract class SignalRIntegrationTestBase : IntegrationTestBase, IAsyncLifetime
{
    private readonly List<SignalRHelper> _signalRConnections = new();

    protected SignalRIntegrationTestBase(CustomWebApplicationFactory factory)
        : base(factory) { }

    /// <summary>
    /// Creates a new SignalR connection with the given access token.
    /// Connection is automatically disposed after the test.
    /// </summary>
    protected async Task<SignalRHelper> CreateSignalRConnectionAsync(string accessToken)
    {
        SignalRHelper connection = new SignalRHelper(GetHubUrl());
        await connection.ConnectAsync(accessToken);
        _signalRConnections.Add(connection);
        return connection;
    }

    public override async Task DisposeAsync()
    {
        // Dispose all SignalR connections
        foreach (SignalRHelper connection in _signalRConnections)
        {
            await connection.DisposeAsync();
        }
        _signalRConnections.Clear();

        await base.DisposeAsync();
    }

    private string GetHubUrl()
    {
        return $"{Factory.Server.BaseAddress}hubs/chat";
    }
}
```

### xUnit Collection Definition

```csharp
[CollectionDefinition("SignalR")]
public class SignalRCollection : ICollectionFixture<CustomWebApplicationFactory>
{
    // This class has no code - it's just a marker for xUnit
    // to understand that all [Collection("SignalR")] classes
    // share the same factory and run sequentially
}
```
