using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Cdm.Migrations;

#nullable disable

namespace Cdm.Migrations.Migrations
{
    /// <summary>
    /// Ajoute la colonne [IsShared] (marketplace) sur [Worlds], [Campaigns] et [Characters].
    /// Migration idempotente (IF COL_LENGTH ... IS NULL).
    /// </summary>
    [DbContext(typeof(MigrationsContext))]
    [Migration("20260721070000_AddMarketplaceSharing")]
    public partial class AddMarketplaceSharing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF COL_LENGTH(N'[Worlds]', N'IsShared') IS NULL
                    ALTER TABLE [Worlds] ADD [IsShared] bit NOT NULL DEFAULT 0;
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH(N'[Campaigns]', N'IsShared') IS NULL
                    ALTER TABLE [Campaigns] ADD [IsShared] bit NOT NULL DEFAULT 0;
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH(N'[Characters]', N'IsShared') IS NULL
                    ALTER TABLE [Characters] ADD [IsShared] bit NOT NULL DEFAULT 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF COL_LENGTH(N'[Worlds]', N'IsShared') IS NOT NULL ALTER TABLE [Worlds] DROP COLUMN [IsShared];");
            migrationBuilder.Sql(@"IF COL_LENGTH(N'[Campaigns]', N'IsShared') IS NOT NULL ALTER TABLE [Campaigns] DROP COLUMN [IsShared];");
            migrationBuilder.Sql(@"IF COL_LENGTH(N'[Characters]', N'IsShared') IS NOT NULL ALTER TABLE [Characters] DROP COLUMN [IsShared];");
        }
    }
}
