using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifyMe.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddObjectivePrerequisites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ObjectiveObjective",
                columns: table => new
                {
                    IsPrerequisiteForId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrerequisitesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectiveObjective", x => new { x.IsPrerequisiteForId, x.PrerequisitesId });
                    table.ForeignKey(
                        name: "FK_ObjectiveObjective_Objectives_IsPrerequisiteForId",
                        column: x => x.IsPrerequisiteForId,
                        principalTable: "Objectives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ObjectiveObjective_Objectives_PrerequisitesId",
                        column: x => x.PrerequisitesId,
                        principalTable: "Objectives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ObjectiveObjective_PrerequisitesId",
                table: "ObjectiveObjective",
                column: "PrerequisitesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ObjectiveObjective");
        }
    }
}
