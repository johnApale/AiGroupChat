# AiGroupChat.Domain

The Domain layer is the core of the application, containing enterprise business logic and entities. This layer has no dependencies on other projects or external frameworks (except for ASP.NET Identity for the User entity).

## Responsibilities

- Define core business entities
- Define enums and value objects
- Define domain interfaces (contracts)
- Remain independent of infrastructure concerns (databases, APIs, etc.)

## Folder Structure

```
AiGroupChat.Domain/
├── Entities/       # Core business objects
├── Enums/          # Enumeration types
└── Interfaces/     # Repository and service contracts (future)
```

## Entities

| Entity               | Description                                             |
| -------------------- | ------------------------------------------------------- |
| `User`               | Application user, extends ASP.NET Identity              |
| `Group`              | Chat group that contains members and messages           |
| `GroupMember`        | Junction entity linking users to groups with roles      |
| `Message`            | A message in a group (from user or AI)                  |
| `AiProvider`         | Configuration for an AI provider (Gemini, Claude, etc.) |
| `AiResponseMetadata` | Token usage and cost tracking for AI responses          |
| `RefreshToken`       | JWT refresh token for authentication                    |

## Enums

| Enum         | Values            | Description                   |
| ------------ | ----------------- | ----------------------------- |
| `SenderType` | `User`, `Ai`      | Identifies who sent a message |
| `GroupRole`  | `Member`, `Admin` | User's role within a group    |

## Design Decisions

1. **User extends IdentityUser** - Leverages ASP.NET Identity for authentication while adding custom fields (`DisplayName`, `CreatedAt`, `UpdatedAt`).

2. **Nullable SenderId on Message** - AI messages don't have a user sender, so `SenderId` is nullable. The `SenderType` enum clarifies the source.

3. **AiVisible flag on Message** - Tracks whether the AI can see this message, based on whether monitoring was enabled when the message was sent.

4. **Separate AiResponseMetadata** - Keeps AI-specific data (tokens, latency, cost) separate from the core Message entity.
