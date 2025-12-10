# Group Owner Controller Integration Tests

## Overview

Integration tests for the `GroupOwnerController` endpoint that handles ownership transfer.

## Endpoints Tested

| Method | Endpoint                      | Description              |
| ------ | ----------------------------- | ------------------------ |
| PUT    | `/api/groups/{groupId}/owner` | Transfer group ownership |

## Test Coverage

### TransferOwnershipTests.cs (9 tests)

| Test                                                | Status Code | Description                  |
| --------------------------------------------------- | ----------- | ---------------------------- |
| `TransferOwnership_AsOwner_ToMember_Returns200`     | 200         | Owner can transfer to member |
| `TransferOwnership_AsOwner_ToAdmin_Returns200`      | 200         | Owner can transfer to admin  |
| `TransferOwnership_AsAdmin_Returns403`              | 403         | Admin cannot transfer        |
| `TransferOwnership_AsMember_Returns403`             | 403         | Member cannot transfer       |
| `TransferOwnership_AsNonMember_Returns403`          | 403         | Non-member cannot transfer   |
| `TransferOwnership_ToSelf_Returns400`               | 400         | Cannot transfer to yourself  |
| `TransferOwnership_ToNonMember_Returns404`          | 404         | New owner must be a member   |
| `TransferOwnership_WithNonExistentGroup_Returns404` | 404         | Group not found              |
| `TransferOwnership_WithoutToken_Returns401`         | 401         | Auth required                |

## Authorization Rules

| Role       | Can Transfer Ownership |
| ---------- | ---------------------- |
| Owner      | ✅                     |
| Admin      | ❌                     |
| Member     | ❌                     |
| Non-Member | ❌                     |

## Business Rules

- Only the current owner can transfer ownership
- New owner must be an existing member of the group
- Cannot transfer ownership to yourself
- After transfer:
  - Previous owner becomes Admin
  - New owner becomes Owner

## Running the Tests

```bash
# Run all GroupOwner controller tests
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~Controllers.GroupOwner"

# Run with verbose output
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~TransferOwnershipTests" --verbosity normal
```

## Notes

- All endpoints require authentication (`[Authorize]` attribute on controller)
- Ownership transfer is atomic - both role changes happen together
- SignalR broadcasts role change events to group members (not tested in integration tests)
