using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Cdm.Migrations;

#nullable disable

namespace Cdm.Migrations.Migrations
{
    /// <summary>
    /// Fait de [DndInventoryItems] l'inventaire unifié multi-systèmes : ajoute [GameType]
    /// (les lignes existantes = D&amp;D = 1), [GameSpecificData] (JSON par thème) et [ImageUrl].
    /// Migration idempotente (IF COL_LENGTH ... IS NULL).
    /// </summary>
    [DbContext(typeof(MigrationsContext))]
    [Migration("20260721090000_UnifyInventoryGameType")]
    public partial class UnifyInventoryGameType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Existing rows are all D&D items → default their GameType to 1 (DnD5e).
            migrationBuilder.Sql(@"
                IF COL_LENGTH(N'[DndInventoryItems]', N'GameType') IS NULL
                    ALTER TABLE [DndInventoryItems] ADD [GameType] int NOT NULL DEFAULT 1;
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH(N'[DndInventoryItems]', N'GameSpecificData') IS NULL
                    ALTER TABLE [DndInventoryItems] ADD [GameSpecificData] nvarchar(max) NULL;
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH(N'[DndInventoryItems]', N'ImageUrl') IS NULL
                    ALTER TABLE [DndInventoryItems] ADD [ImageUrl] nvarchar(1000) NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF COL_LENGTH(N'[DndInventoryItems]', N'ImageUrl') IS NOT NULL ALTER TABLE [DndInventoryItems] DROP COLUMN [ImageUrl];");
            migrationBuilder.Sql(@"IF COL_LENGTH(N'[DndInventoryItems]', N'GameSpecificData') IS NOT NULL ALTER TABLE [DndInventoryItems] DROP COLUMN [GameSpecificData];");
            migrationBuilder.Sql(@"IF COL_LENGTH(N'[DndInventoryItems]', N'GameType') IS NOT NULL ALTER TABLE [DndInventoryItems] DROP COLUMN [GameType];");
        }
    }
}
