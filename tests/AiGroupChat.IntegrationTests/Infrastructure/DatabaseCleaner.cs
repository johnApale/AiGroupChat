using AiGroupChat.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AiGroupChat.IntegrationTests.Infrastructure;

/// <summary>
/// Handles database cleanup between tests
/// </summary>
public static class DatabaseCleaner
{
    /// <summary>
    /// Deletes all data from tables in the correct order (respecting foreign keys)
    /// </summary>
    public static async Task CleanAsync(IServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Order matters due to foreign key constraints
        await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM ai_response_metadata");
        await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM messages");
        await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM group_members");
        await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM groups");
        await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM refresh_tokens");
        await dbContext.Database.ExecuteSqlRawAsync(@"DELETE FROM ""AspNetUserTokens""");
        await dbContext.Database.ExecuteSqlRawAsync(@"DELETE FROM ""AspNetUserLogins""");
        await dbContext.Database.ExecuteSqlRawAsync(@"DELETE FROM ""AspNetUserRoles""");
        await dbContext.Database.ExecuteSqlRawAsync(@"DELETE FROM ""AspNetUsers""");
    }
}