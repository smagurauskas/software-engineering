using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordGame.Data.Migrations
{
    /// <inheritdoc />
    // #region migration
    public partial class AddUniqueEmailIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_email",
                table: "users");
        }
    }
    // #endregion migration
}
