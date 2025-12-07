using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifyMe.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsOnboardingProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Migrate existing Onboarding items to Category = 3 (Onboarding)
            migrationBuilder.Sql("UPDATE \"Objectives\" SET \"Category\" = 3 WHERE \"IsOnboarding\" = true;");

            migrationBuilder.DropColumn(
                name: "IsOnboarding",
                table: "Objectives");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOnboarding",
                table: "Objectives",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
