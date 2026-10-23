using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordGame.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGameStartedAtIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_games_started_at",
                table: "games",
                column: "started_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_games_started_at",
                table: "games");
        }
    }
}
