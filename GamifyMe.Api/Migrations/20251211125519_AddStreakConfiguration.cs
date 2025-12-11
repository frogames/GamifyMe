using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifyMe.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStreakConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StreakExcludedDays",
                table: "Objectives",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StreakExcludedMonths",
                table: "Objectives",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StreakFrequency",
                table: "Objectives",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StreakExcludedDays",
                table: "Objectives");

            migrationBuilder.DropColumn(
                name: "StreakExcludedMonths",
                table: "Objectives");

            migrationBuilder.DropColumn(
                name: "StreakFrequency",
                table: "Objectives");
        }
    }
}
