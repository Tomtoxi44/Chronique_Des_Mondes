using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Cdm.Migrations;

#nullable disable

namespace Cdm.Migrations.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(MigrationsContext))]
    [Migration("20260418120000_AddWorldCharactersTable")]
    public partial class AddWorldCharactersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent: only create if missing (table was in InitialCreate snapshot
            // but may not have been applied to existing databases)
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[WorldCharacters]', N'U') IS NULL
BEGIN
    CREATE TABLE [WorldCharacters] (
        [Id] int NOT NULL IDENTITY(1,1),
        [CharacterId] int NOT NULL,
        [WorldId] int NOT NULL,
        [GameSpecificData] nvarchar(max) NULL,
        [Level] int NULL,
        [CurrentHealth] int NULL,
        [MaxHealth] int NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [JoinedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_WorldCharacters] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WorldCharacters_Characters_CharacterId] FOREIGN KEY ([CharacterId])
            REFERENCES [Characters] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_WorldCharacters_Worlds_WorldId] FOREIGN KEY ([WorldId])
            REFERENCES [Worlds] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_WorldCharacters_CharacterId]
        ON [WorldCharacters] ([CharacterId]);

    CREATE UNIQUE INDEX [IX_WorldCharacters_CharacterId_WorldId]
        ON [WorldCharacters] ([CharacterId], [WorldId]);

    CREATE INDEX [IX_WorldCharacters_WorldId]
        ON [WorldCharacters] ([WorldId]);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[WorldCharacters]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [WorldCharacters];
END
");
        }
    }
}
