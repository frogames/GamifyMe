using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifyMe.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEstablishmentSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrencyName",
                table: "Establishments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsChallengesEnabled",
                table: "Establishments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsGroupsEnabled",
                table: "Establishments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsShopEnabled",
                table: "Establishments",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrencyName",
                table: "Establishments");

            migrationBuilder.DropColumn(
                name: "IsChallengesEnabled",
                table: "Establishments");

            migrationBuilder.DropColumn(
                name: "IsGroupsEnabled",
                table: "Establishments");

            migrationBuilder.DropColumn(
                name: "IsShopEnabled",
                table: "Establishments");
        }
    }
}
