using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifyMe.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Users\" SET \"Role\" = 'Staff' WHERE \"Role\" = 'Gestionnaire';");
            migrationBuilder.Sql("UPDATE \"Users\" SET \"Role\" = 'Coach' WHERE \"Role\" = 'Editeur';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Users\" SET \"Role\" = 'Gestionnaire' WHERE \"Role\" = 'Staff';");
            migrationBuilder.Sql("UPDATE \"Users\" SET \"Role\" = 'Editeur' WHERE \"Role\" = 'Coach';");
        }
    }
}
