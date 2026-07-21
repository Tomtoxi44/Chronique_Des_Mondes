using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Cdm.Migrations;

#nullable disable

namespace Cdm.Migrations.Migrations
{
    /// <summary>
    /// Migre les inventaires « legacy » (ancien blob JSON <c>$.inventory</c> = [{name, qty}] stocké
    /// dans <c>WorldCharacters.GameSpecificData</c>) vers la table d'inventaire unifiée
    /// <c>DndInventoryItems</c> (items génériques), puis retire la clé <c>inventory</c> du JSON.
    /// Idempotent : après retrait de la clé, une réexécution ne trouve plus rien à migrer.
    /// </summary>
    [DbContext(typeof(MigrationsContext))]
    [Migration("20260721100000_MigrateLegacyInventory")]
    public partial class MigrateLegacyInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Copie chaque objet legacy en item d'inventaire générique (GameType = 0).
            migrationBuilder.Sql(@"
                INSERT INTO [DndInventoryItems] ([WorldCharacterId], [Name], [Category], [Quantity], [GameType], [CreatedAt])
                SELECT wc.[Id],
                       LEFT(j.[name], 200),
                       N'Objet',
                       CASE WHEN j.[qty] IS NULL OR j.[qty] < 1 THEN 1 ELSE j.[qty] END,
                       0,
                       GETUTCDATE()
                FROM [WorldCharacters] wc
                CROSS APPLY OPENJSON(wc.[GameSpecificData], '$.inventory')
                    WITH ([name] nvarchar(200) '$.name', [qty] int '$.qty') j
                WHERE ISJSON(wc.[GameSpecificData]) = 1
                  AND JSON_QUERY(wc.[GameSpecificData], '$.inventory') IS NOT NULL
                  AND j.[name] IS NOT NULL
                  AND LTRIM(RTRIM(j.[name])) <> N'';
            ");

            // 2) Retire la clé inventory du JSON (rend l'opération idempotente).
            migrationBuilder.Sql(@"
                UPDATE [WorldCharacters]
                SET [GameSpecificData] = JSON_MODIFY([GameSpecificData], '$.inventory', NULL)
                WHERE ISJSON([GameSpecificData]) = 1
                  AND JSON_QUERY([GameSpecificData], '$.inventory') IS NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Migration de données irréversible : on ne recompose pas le blob JSON.
        }
    }
}
