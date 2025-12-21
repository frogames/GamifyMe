using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifyMe.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddContentKits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentKits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    TemplateEstablishmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    HasObjectives = table.Column<bool>(type: "boolean", nullable: false),
                    HasBadges = table.Column<bool>(type: "boolean", nullable: false),
                    HasGroups = table.Column<bool>(type: "boolean", nullable: false),
                    HasStoreItems = table.Column<bool>(type: "boolean", nullable: false),
                    UsageCount = table.Column<int>(type: "integer", nullable: false),
                    AverageRating = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentKits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentKits_Establishments_TemplateEstablishmentId",
                        column: x => x.TemplateEstablishmentId,
                        principalTable: "Establishments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KitRatings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KitId = table.Column<Guid>(type: "uuid", nullable: false),
                    EstablishmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KitRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KitRatings_ContentKits_KitId",
                        column: x => x.KitId,
                        principalTable: "ContentKits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentKits_TemplateEstablishmentId",
                table: "ContentKits",
                column: "TemplateEstablishmentId");

            migrationBuilder.CreateIndex(
                name: "IX_KitRatings_KitId",
                table: "KitRatings",
                column: "KitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KitRatings");

            migrationBuilder.DropTable(
                name: "ContentKits");
        }
    }
}
