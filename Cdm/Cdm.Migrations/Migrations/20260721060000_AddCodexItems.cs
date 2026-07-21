using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Cdm.Migrations;

#nullable disable

namespace Cdm.Migrations.Migrations
{
    /// <summary>
    /// Ajoute la table [CodexItems] : le codex personnel d'items d'un utilisateur
    /// (modèles génériques cross-thème, stats spécifiques en JSON), destinés à être
    /// copiés dans l'inventaire d'un personnage ou partagés sur la marketplace.
    /// Migration idempotente (IF OBJECT_ID ... IS NULL).
    /// </summary>
    [DbContext(typeof(MigrationsContext))]
    [Migration("20260721060000_AddCodexItems")]
    public partial class AddCodexItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[CodexItems]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [CodexItems] (
                        [Id] int NOT NULL IDENTITY,
                        [UserId] int NOT NULL,
                        [Name] nvarchar(200) NOT NULL DEFAULT N'',
                        [Description] nvarchar(max) NULL,
                        [ImageUrl] nvarchar(1000) NULL,
                        [GameType] int NOT NULL DEFAULT 0,
                        [ItemType] nvarchar(50) NULL,
                        [GameSpecificData] nvarchar(max) NULL,
                        [IsShared] bit NOT NULL DEFAULT 0,
                        [IsActive] bit NOT NULL DEFAULT 1,
                        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
                        [UpdatedAt] datetime2 NULL,
                        CONSTRAINT [PK_CodexItems] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_CodexItems_Users_UserId] FOREIGN KEY ([UserId])
                            REFERENCES [Users] ([Id]) ON DELETE CASCADE
                    );

                    CREATE INDEX [IX_CodexItems_UserId] ON [CodexItems] ([UserId]);
                    CREATE INDEX [IX_CodexItems_GameType] ON [CodexItems] ([GameType]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[CodexItems]', N'U') IS NOT NULL
                    DROP TABLE [CodexItems];
            ");
        }
    }
}
