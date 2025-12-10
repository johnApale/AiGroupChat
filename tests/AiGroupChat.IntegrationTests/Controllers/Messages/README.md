# Messages Controller Integration Tests

## Overview

Integration tests for the `MessagesController` endpoints that handle sending and retrieving messages in groups.

## Endpoints Tested

| Method | Endpoint                         | Description              |
| ------ | -------------------------------- | ------------------------ |
| POST   | `/api/groups/{groupId}/messages` | Send message to group    |
| GET    | `/api/groups/{groupId}/messages` | Get messages (paginated) |

## Test Coverage

### SendMessageTests.cs (7 tests)

| Test                                          | Status Code | Description             |
| --------------------------------------------- | ----------- | ----------------------- |
| `SendMessage_AsMember_Returns201`             | 201         | Owner can send message  |
| `SendMessage_AsRegularMember_Returns201`      | 201         | Regular member can send |
| `SendMessage_AsNonMember_Returns403`          | 403         | Non-member cannot send  |
| `SendMessage_WithEmptyContent_Returns400`     | 400         | Content required        |
| `SendMessage_WithTooLongContent_Returns400`   | 400         | Content max 10000 chars |
| `SendMessage_WithNonExistentGroup_Returns404` | 404         | Group not found         |
| `SendMessage_WithoutToken_Returns401`         | 401         | Auth required           |

### GetMessagesTests.cs (9 tests)

| Test                                                  | Status Code | Description                    |
| ----------------------------------------------------- | ----------- | ------------------------------ |
| `GetMessages_AsMember_ReturnsMessages`                | 200         | Owner can view messages        |
| `GetMessages_AsRegularMember_ReturnsMessages`         | 200         | Regular member can view        |
| `GetMessages_WithPagination_ReturnsCorrectPage`       | 200         | Page 1 pagination works        |
| `GetMessages_WithPagination_Page2_ReturnsCorrectPage` | 200         | Page 2 pagination works        |
| `GetMessages_EmptyGroup_ReturnsEmptyList`             | 200         | Empty group returns empty list |
| `GetMessages_AsNonMember_Returns403`                  | 403         | Non-member cannot view         |
| `GetMessages_WithNonExistentGroup_Returns404`         | 404         | Group not found                |
| `GetMessages_WithoutToken_Returns401`                 | 401         | Auth required                  |

## Authorization Rules

| Role       | Can Send | Can View |
| ---------- | -------- | -------- |
| Owner      | ✅       | ✅       |
| Admin      | ✅       | ✅       |
| Member     | ✅       | ✅       |
| Non-Member | ❌       | ❌       |

## Pagination

The `GetMessages` endpoint supports pagination with the following query parameters:

| Parameter  | Default | Min | Max | Description    |
| ---------- | ------- | --- | --- | -------------- |
| `page`     | 1       | 1   | -   | Page number    |
| `pageSize` | 50      | 1   | 100 | Items per page |

Response includes pagination metadata:

- `page` - Current page number
- `pageSize` - Items per page
- `totalCount` - Total number of messages
- `totalPages` - Total number of pages
- `hasNextPage` - Whether there's a next page
- `hasPreviousPage` - Whether there's a previous page

## Running the Tests

```bash
# Run all Messages controller tests
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~Controllers.Messages"

# Run specific test file
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~SendMessageTests"
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~GetMessagesTests"

# Run with verbose output
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~Controllers.Messages" --verbosity normal
```

## Notes

- All endpoints require authentication (`[Authorize]` attribute on controller)
- Any group member can send and view messages
- Message content is required and limited to 10,000 characters
- Messages are stored with `AiVisible` flag based on group's `AiMonitoringEnabled` setting
- SignalR broadcasts messages to group members (not tested in integration tests)
