using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiGroupChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedAiProviders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            DateTime now = DateTime.UtcNow;

            migrationBuilder.InsertData(
                table: "ai_providers",
                columns: new[] { "id", "name", "display_name", "is_enabled", "sort_order", "base_url", "default_model", "default_temperature", "max_tokens_limit", "input_token_cost", "output_token_cost", "created_at", "updated_at" },
                values: new object[,]
                {
                    { Guid.Parse("11111111-1111-1111-1111-111111111111"), "gemini", "Google Gemini", true, 0, null, "gemini-1.5-pro", 0.7m, 1000000, 0.00025m, 0.0005m, now, now },
                    { Guid.Parse("22222222-2222-2222-2222-222222222222"), "claude", "Anthropic Claude", true, 1, null, "claude-3-5-sonnet-20241022", 0.7m, 200000, 0.003m, 0.015m, now, now },
                    { Guid.Parse("33333333-3333-3333-3333-333333333333"), "openai", "OpenAI", true, 2, null, "gpt-4o", 0.7m, 128000, 0.0025m, 0.01m, now, now },
                    { Guid.Parse("44444444-4444-4444-4444-444444444444"), "grok", "xAI Grok", true, 3, null, "grok-2", 0.7m, 131072, 0.002m, 0.01m, now, now }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ai_providers",
                keyColumn: "id",
                keyValues: new object[]
                {
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Guid.Parse("44444444-4444-4444-4444-444444444444")
                });
        }
    }
}
