using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifyMe.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixMissingStoreDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This migration is manually added to fix missing columns in production
            // because the previous migration was skipped/baselined incorrectly.
            
            // We use SQL to check existence to be safe, although AddColumn usually throws if exists.
            // But since we are fixing a desync, let's just try to add them.
            // If they already exist (e.g. in dev), this might fail in dev but succeed in prod.
            // To be safe for BOTH environments, we can check column existence via SQL if we want,
            // but EF Core migrations are usually declarative.
            
            // Given the user's situation: Prod is missing them, Dev has them.
            // If I run this in Dev, it will crash.
            // So I should wrap it in a raw SQL check.
            
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='StoreItems' AND column_name='EndDate') THEN
                        ALTER TABLE ""StoreItems"" ADD COLUMN ""EndDate"" timestamp with time zone NULL;
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='StoreItems' AND column_name='StartDate') THEN
                        ALTER TABLE ""StoreItems"" ADD COLUMN ""StartDate"" timestamp with time zone NULL;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // We don't want to drop them automatically as they might be data-critical
            // But for correctness:
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "StoreItems");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "StoreItems");
        }
    }
}
