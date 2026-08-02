using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clausio.Legal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReadinessSummaryGapsStrengths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GapsJson",
                table: "Readinesses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StrengthsJson",
                table: "Readinesses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "Readinesses",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GapsJson",
                table: "Readinesses");

            migrationBuilder.DropColumn(
                name: "StrengthsJson",
                table: "Readinesses");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Readinesses");
        }
    }
}
