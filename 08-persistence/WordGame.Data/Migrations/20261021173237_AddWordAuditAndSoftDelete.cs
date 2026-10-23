using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordGame.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWordAuditAndSoftDelete : Migration
    {
        /// <inheritdoc />
        // #region up
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Edited by hand: the generated migration set the timestamps of existing words
            // to 0001-01-01 (the default of DateTimeOffset). now() is a better guess.
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "words",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "words",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "words",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");
        }
        // #endregion up

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                table: "words");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "words");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "words");
        }
    }
}
