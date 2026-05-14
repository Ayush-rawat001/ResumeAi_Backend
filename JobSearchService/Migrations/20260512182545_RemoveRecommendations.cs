using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSearchService.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRecommendations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Recommendations",
                table: "JobMatches");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Recommendations",
                table: "JobMatches",
                type: "text",
                nullable: true);
        }
    }
}
