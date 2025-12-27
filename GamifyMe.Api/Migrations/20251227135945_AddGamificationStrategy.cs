using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifyMe.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddGamificationStrategy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GamificationStrategies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EstablishmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EngagementEngine = table.Column<int>(type: "integer", nullable: false),
                    FeedbackFrequency = table.Column<int>(type: "integer", nullable: false),
                    CycleType = table.Column<int>(type: "integer", nullable: false),
                    CycleDurationMonths = table.Column<int>(type: "integer", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamificationStrategies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GamificationStrategies_Establishments_EstablishmentId",
                        column: x => x.EstablishmentId,
                        principalTable: "Establishments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GamificationStrategies_EstablishmentId",
                table: "GamificationStrategies",
                column: "EstablishmentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GamificationStrategies");
        }
    }
}
