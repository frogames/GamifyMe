using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifyMe.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryToObjective : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Objectives",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Objectives");
        }
    }
}
