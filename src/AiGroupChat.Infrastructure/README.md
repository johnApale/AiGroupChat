# AiGroupChat.Infrastructure

The Infrastructure layer implements external concerns and data access. This layer contains concrete implementations of interfaces defined in the Application layer.

## Responsibilities

- Database access via Entity Framework Core
- Entity configurations and migrations
- External service integrations (AI client, email, etc.)
- Repository implementations

## Dependencies

- **AiGroupChat.Application** (and transitively Domain)
- **Npgsql.EntityFrameworkCore.PostgreSQL** - PostgreSQL database provider
- **Microsoft.AspNetCore.Identity.EntityFrameworkCore** - Identity with EF Core
- **Microsoft.EntityFrameworkCore.Tools** - Migration tooling

## Folder Structure

```
AiGroupChat.Infrastructure/
├── Data/
│   ├── Configurations/     # EF Core entity configurations
│   └── ApplicationDbContext.cs
├── DependencyInjection.cs  # Service registration
└── README.md
```

## Entity Configurations

Each entity has a corresponding configuration class that defines:

- Table name (snake_case convention)
- Column mappings and types
- Indexes and constraints
- Relationships and foreign keys

| Configuration                     | Table                  | Description                    |
| --------------------------------- | ---------------------- | ------------------------------ |
| `AiProviderConfiguration`         | `ai_providers`         | AI provider settings and costs |
| `UserConfiguration`               | `AspNetUsers`          | Extends Identity user table    |
| `RefreshTokenConfiguration`       | `refresh_tokens`       | JWT refresh tokens             |
| `GroupConfiguration`              | `groups`               | Chat groups                    |
| `GroupMemberConfiguration`        | `group_members`        | User-group membership          |
| `MessageConfiguration`            | `messages`             | Chat messages                  |
| `AiResponseMetadataConfiguration` | `ai_response_metadata` | AI response tracking           |

## Database Conventions

- **Table names**: snake_case (e.g., `group_members`)
- **Column names**: snake_case (e.g., `created_at`)
- **Enums**: Stored as strings for readability
- **Soft deletes**: Not implemented (hard deletes used)
- **Timestamps**: `CreatedAt` and `UpdatedAt` on most entities

## Usage

Register infrastructure services in `Program.cs`:

```csharp
using AiGroupChat.Infrastructure;

builder.Services.AddInfrastructure(builder.Configuration);
```

## Connection String

Configure in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5434;Database=aigroupchat_dev;Username=aigroupchat;Password=devpassword123"
  }
}
```

## Migrations

```bash
# Add a new migration
dotnet ef migrations add <MigrationName> \
  --project src/AiGroupChat.Infrastructure \
  --startup-project src/AiGroupChat.API

# Apply migrations
dotnet ef database update \
  --project src/AiGroupChat.Infrastructure \
  --startup-project src/AiGroupChat.API

# Remove last migration (if not applied)
dotnet ef migrations remove \
  --project src/AiGroupChat.Infrastructure \
  --startup-project src/AiGroupChat.API
```

## Design Decisions

1. **Snake_case naming** - PostgreSQL convention for table and column names, mapped from C# PascalCase properties.

2. **Configurations in separate files** - Each entity has its own configuration class for maintainability.

3. **Enum as string** - `SenderType` and `GroupRole` stored as strings for database readability and easier debugging.

4. **Composite indexes** - Added on frequently queried combinations (e.g., `GroupId + AiVisible` for fetching AI context).

5. **Cascade deletes** - Messages deleted when group is deleted; metadata deleted when message is deleted.
