# Invitations Integration Tests (Public Endpoints)

Integration tests for the public invitation acceptance endpoint.

## Test Coverage

### AcceptInvitationTests

| Test                                                        | Description                                     |
| ----------------------------------------------------------- | ----------------------------------------------- |
| AcceptInvitation_WithExistingUser_AddsToGroupAndReturnsAuth | Existing user is added and receives auth tokens |
| AcceptInvitation_WithNewUser_ReturnsRequiresRegistration    | New user gets registration prompt               |
| AcceptInvitation_WithInvalidToken_Returns404                | Invalid tokens rejected                         |
| AcceptInvitation_WithRevokedInvitation_Returns400           | Revoked invitations rejected                    |
| AcceptInvitation_WhenAlreadyMember_Returns400               | Already-members cannot accept                   |
| AcceptInvitation_WithAlreadyAcceptedInvitation_Returns400   | Cannot accept twice                             |
| AcceptInvitation_DoesNotRequireAuthentication               | Endpoint is publicly accessible                 |

## API Endpoints Tested

| Method | Endpoint                  | Description                |
| ------ | ------------------------- | -------------------------- |
| POST   | `/api/invitations/accept` | Accept invitation (public) |

## Running Tests

```bash
# Run all Invitations tests
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~Invitations.AcceptInvitation"
```
