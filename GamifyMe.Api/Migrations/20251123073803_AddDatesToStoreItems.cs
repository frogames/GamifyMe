using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifyMe.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDatesToStoreItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // MIGRATION SKIPPED (Baselining)
            /*
            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "StoreItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "StoreItems",
                type: "timestamp with time zone",
                nullable: true);
            */
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "StoreItems");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "StoreItems");
        }
    }
}
