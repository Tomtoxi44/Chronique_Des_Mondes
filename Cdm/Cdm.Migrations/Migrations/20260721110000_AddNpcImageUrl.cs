using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Cdm.Migrations;

#nullable disable

namespace Cdm.Migrations.Migrations
{
    /// <summary>
    /// Ajoute la colonne [ImageUrl] (portrait) sur [NonPlayerCharacters].
    /// Migration idempotente (IF COL_LENGTH ... IS NULL).
    /// </summary>
    [DbContext(typeof(MigrationsContext))]
    [Migration("20260721110000_AddNpcImageUrl")]
    public partial class AddNpcImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF COL_LENGTH(N'[NonPlayerCharacters]', N'ImageUrl') IS NULL
                    ALTER TABLE [NonPlayerCharacters] ADD [ImageUrl] nvarchar(1000) NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF COL_LENGTH(N'[NonPlayerCharacters]', N'ImageUrl') IS NOT NULL ALTER TABLE [NonPlayerCharacters] DROP COLUMN [ImageUrl];");
        }
    }
}
