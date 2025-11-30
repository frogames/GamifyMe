using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifyMe.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddValidatedByNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ValidatedById",
                table: "Validations",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            // Fix existing data: Set ValidatedById to NULL where it is Guid.Empty
            migrationBuilder.Sql("UPDATE \"Validations\" SET \"ValidatedById\" = NULL WHERE \"ValidatedById\" = '00000000-0000-0000-0000-000000000000'");

            migrationBuilder.CreateIndex(
                name: "IX_Validations_ValidatedById",
                table: "Validations",
                column: "ValidatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Validations_Users_ValidatedById",
                table: "Validations",
                column: "ValidatedById",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Validations_Users_ValidatedById",
                table: "Validations");

            migrationBuilder.DropIndex(
                name: "IX_Validations_ValidatedById",
                table: "Validations");

            migrationBuilder.AlterColumn<Guid>(
                name: "ValidatedById",
                table: "Validations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
