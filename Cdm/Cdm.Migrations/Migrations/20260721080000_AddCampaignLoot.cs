using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Cdm.Migrations;

#nullable disable

namespace Cdm.Migrations.Migrations
{
    /// <summary>
    /// Ajoute les tables [CampaignLoots] (loot préparé par le MJ pour une campagne/chapitre)
    /// et [LootDistributions] (traçabilité de la distribution aux personnages joueurs).
    /// Migration idempotente (IF OBJECT_ID ... IS NULL). Les FK vers WorldCharacters/Chapters
    /// sont en NO ACTION pour éviter les chemins de cascade multiples SQL Server.
    /// </summary>
    [DbContext(typeof(MigrationsContext))]
    [Migration("20260721080000_AddCampaignLoot")]
    public partial class AddCampaignLoot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[CampaignLoots]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [CampaignLoots] (
                        [Id] int NOT NULL IDENTITY,
                        [CampaignId] int NOT NULL,
                        [ChapterId] int NULL,
                        [Name] nvarchar(200) NOT NULL DEFAULT N'',
                        [Description] nvarchar(max) NULL,
                        [ImageUrl] nvarchar(1000) NULL,
                        [GameType] int NOT NULL DEFAULT 0,
                        [ItemType] nvarchar(50) NULL,
                        [GameSpecificData] nvarchar(max) NULL,
                        [Quantity] int NOT NULL DEFAULT 1,
                        [CreatedBy] int NOT NULL,
                        [IsActive] bit NOT NULL DEFAULT 1,
                        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
                        [UpdatedAt] datetime2 NULL,
                        CONSTRAINT [PK_CampaignLoots] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_CampaignLoots_Campaigns_CampaignId] FOREIGN KEY ([CampaignId])
                            REFERENCES [Campaigns] ([Id]) ON DELETE CASCADE,
                        CONSTRAINT [FK_CampaignLoots_Chapters_ChapterId] FOREIGN KEY ([ChapterId])
                            REFERENCES [Chapters] ([Id]) ON DELETE NO ACTION
                    );

                    CREATE INDEX [IX_CampaignLoots_CampaignId] ON [CampaignLoots] ([CampaignId]);
                    CREATE INDEX [IX_CampaignLoots_ChapterId] ON [CampaignLoots] ([ChapterId]);
                END
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[LootDistributions]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [LootDistributions] (
                        [Id] int NOT NULL IDENTITY,
                        [LootId] int NOT NULL,
                        [WorldCharacterId] int NOT NULL,
                        [SessionId] int NULL,
                        [DistributedByUserId] int NOT NULL,
                        [DistributedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
                        CONSTRAINT [PK_LootDistributions] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_LootDistributions_CampaignLoots_LootId] FOREIGN KEY ([LootId])
                            REFERENCES [CampaignLoots] ([Id]) ON DELETE CASCADE,
                        CONSTRAINT [FK_LootDistributions_WorldCharacters_WorldCharacterId] FOREIGN KEY ([WorldCharacterId])
                            REFERENCES [WorldCharacters] ([Id]) ON DELETE NO ACTION
                    );

                    CREATE INDEX [IX_LootDistributions_LootId] ON [LootDistributions] ([LootId]);
                    CREATE INDEX [IX_LootDistributions_WorldCharacterId] ON [LootDistributions] ([WorldCharacterId]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF OBJECT_ID(N'[LootDistributions]', N'U') IS NOT NULL DROP TABLE [LootDistributions];");
            migrationBuilder.Sql(@"IF OBJECT_ID(N'[CampaignLoots]', N'U') IS NOT NULL DROP TABLE [CampaignLoots];");
        }
    }
}
