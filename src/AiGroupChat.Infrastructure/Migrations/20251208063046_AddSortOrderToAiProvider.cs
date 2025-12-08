using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiGroupChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSortOrderToAiProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "sort_order",
                table: "ai_providers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sort_order",
                table: "ai_providers");
        }
    }
}
