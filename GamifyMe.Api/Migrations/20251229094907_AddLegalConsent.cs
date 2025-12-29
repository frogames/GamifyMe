using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifyMe.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLegalConsent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TermsAcceptedAt",
                table: "Users",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TermsVersionAccepted",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TermsAcceptedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TermsVersionAccepted",
                table: "Users");
        }
    }
}
