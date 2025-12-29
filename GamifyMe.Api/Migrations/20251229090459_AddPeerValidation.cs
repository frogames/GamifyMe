using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifyMe.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPeerValidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequiredPeerValidations",
                table: "Objectives",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PeerObjectiveSignatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EstablishmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectiveId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WitnessUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SignedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeerObjectiveSignatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeerObjectiveSignatures_Objectives_ObjectiveId",
                        column: x => x.ObjectiveId,
                        principalTable: "Objectives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PeerObjectiveSignatures_Users_PerformerUserId",
                        column: x => x.PerformerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PeerObjectiveSignatures_Users_WitnessUserId",
                        column: x => x.WitnessUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PeerObjectiveSignatures_ObjectiveId",
                table: "PeerObjectiveSignatures",
                column: "ObjectiveId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerObjectiveSignatures_PerformerUserId",
                table: "PeerObjectiveSignatures",
                column: "PerformerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerObjectiveSignatures_WitnessUserId",
                table: "PeerObjectiveSignatures",
                column: "WitnessUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PeerObjectiveSignatures");

            migrationBuilder.DropColumn(
                name: "RequiredPeerValidations",
                table: "Objectives");
        }
    }
}
