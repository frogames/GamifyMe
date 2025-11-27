using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifyMe.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDisplayDatesToObjective : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.AddColumn<DateTime>(...);
            // migrationBuilder.AddColumn<DateTime>(...);

            // Use raw SQL to add columns safely if they don't exist
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Objectives' AND column_name='DisplayEndDate') THEN
                        ALTER TABLE ""Objectives"" ADD COLUMN ""DisplayEndDate"" timestamp with time zone;
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Objectives' AND column_name='DisplayStartDate') THEN
                        ALTER TABLE ""Objectives"" ADD COLUMN ""DisplayStartDate"" timestamp with time zone;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayEndDate",
                table: "Objectives");

            migrationBuilder.DropColumn(
                name: "DisplayStartDate",
                table: "Objectives");
        }
    }
}
