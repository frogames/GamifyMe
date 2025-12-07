using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifyMe.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNextOnboardingObjectiveId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Objectives_Objectives_NextOnboardingObjectiveId",
                table: "Objectives");

            migrationBuilder.DropIndex(
                name: "IX_Objectives_NextOnboardingObjectiveId",
                table: "Objectives");

            migrationBuilder.DropColumn(
                name: "NextOnboardingObjectiveId",
                table: "Objectives");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "NextOnboardingObjectiveId",
                table: "Objectives",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Objectives_NextOnboardingObjectiveId",
                table: "Objectives",
                column: "NextOnboardingObjectiveId");

            migrationBuilder.AddForeignKey(
                name: "FK_Objectives_Objectives_NextOnboardingObjectiveId",
                table: "Objectives",
                column: "NextOnboardingObjectiveId",
                principalTable: "Objectives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
