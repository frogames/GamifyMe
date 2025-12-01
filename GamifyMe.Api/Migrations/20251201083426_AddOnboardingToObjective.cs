using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifyMe.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOnboardingToObjective : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentXp",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsOnboarding",
                table: "Objectives",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "NextOnboardingObjectiveId",
                table: "Objectives",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserObjectives",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EstablishmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectiveId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Progress = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserObjectives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserObjectives_Objectives_ObjectiveId",
                        column: x => x.ObjectiveId,
                        principalTable: "Objectives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserObjectives_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Objectives_NextOnboardingObjectiveId",
                table: "Objectives",
                column: "NextOnboardingObjectiveId");

            migrationBuilder.CreateIndex(
                name: "IX_UserObjectives_ObjectiveId",
                table: "UserObjectives",
                column: "ObjectiveId");

            migrationBuilder.CreateIndex(
                name: "IX_UserObjectives_UserId",
                table: "UserObjectives",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Objectives_Objectives_NextOnboardingObjectiveId",
                table: "Objectives",
                column: "NextOnboardingObjectiveId",
                principalTable: "Objectives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Objectives_Objectives_NextOnboardingObjectiveId",
                table: "Objectives");

            migrationBuilder.DropTable(
                name: "UserObjectives");

            migrationBuilder.DropIndex(
                name: "IX_Objectives_NextOnboardingObjectiveId",
                table: "Objectives");

            migrationBuilder.DropColumn(
                name: "CurrentXp",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsOnboarding",
                table: "Objectives");

            migrationBuilder.DropColumn(
                name: "NextOnboardingObjectiveId",
                table: "Objectives");
        }
    }
}
