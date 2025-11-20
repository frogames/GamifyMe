using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifyMe.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixOrderAndValidationModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ObjectiveObjectives_Objectives_IsPrerequisiteForId",
                table: "ObjectiveObjectives");

            migrationBuilder.DropForeignKey(
                name: "FK_ObjectiveObjectives_Objectives_PrerequisitesId",
                table: "ObjectiveObjectives");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ObjectiveObjectives",
                table: "ObjectiveObjectives");

            migrationBuilder.RenameTable(
                name: "ObjectiveObjectives",
                newName: "ObjectiveObjective");

            migrationBuilder.RenameIndex(
                name: "IX_ObjectiveObjectives_PrerequisitesId",
                table: "ObjectiveObjective",
                newName: "IX_ObjectiveObjective_PrerequisitesId");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCompleted",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ObjectiveObjective",
                table: "ObjectiveObjective",
                columns: new[] { "IsPrerequisiteForId", "PrerequisitesId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ObjectiveObjective_Objectives_IsPrerequisiteForId",
                table: "ObjectiveObjective",
                column: "IsPrerequisiteForId",
                principalTable: "Objectives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ObjectiveObjective_Objectives_PrerequisitesId",
                table: "ObjectiveObjective",
                column: "PrerequisitesId",
                principalTable: "Objectives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ObjectiveObjective_Objectives_IsPrerequisiteForId",
                table: "ObjectiveObjective");

            migrationBuilder.DropForeignKey(
                name: "FK_ObjectiveObjective_Objectives_PrerequisitesId",
                table: "ObjectiveObjective");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ObjectiveObjective",
                table: "ObjectiveObjective");

            migrationBuilder.DropColumn(
                name: "DateCompleted",
                table: "Orders");

            migrationBuilder.RenameTable(
                name: "ObjectiveObjective",
                newName: "ObjectiveObjectives");

            migrationBuilder.RenameIndex(
                name: "IX_ObjectiveObjective_PrerequisitesId",
                table: "ObjectiveObjectives",
                newName: "IX_ObjectiveObjectives_PrerequisitesId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ObjectiveObjectives",
                table: "ObjectiveObjectives",
                columns: new[] { "IsPrerequisiteForId", "PrerequisitesId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ObjectiveObjectives_Objectives_IsPrerequisiteForId",
                table: "ObjectiveObjectives",
                column: "IsPrerequisiteForId",
                principalTable: "Objectives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ObjectiveObjectives_Objectives_PrerequisitesId",
                table: "ObjectiveObjectives",
                column: "PrerequisitesId",
                principalTable: "Objectives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
