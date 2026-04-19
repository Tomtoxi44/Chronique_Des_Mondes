using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cdm.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class EnsureSessionTablesAndIsLocked : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent: add IsLocked to Characters if missing
            migrationBuilder.Sql(@"
IF COL_LENGTH('[dbo].[Characters]', 'IsLocked') IS NULL
BEGIN
    ALTER TABLE [Characters] ADD [IsLocked] bit NOT NULL DEFAULT 0;
    CREATE INDEX [IX_Characters_IsLocked] ON [Characters] ([IsLocked]);
END");

            // Idempotent: create Sessions table if missing
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[Sessions]', N'U') IS NULL
BEGIN
    CREATE TABLE [Sessions] (
        [Id] int NOT NULL IDENTITY,
        [CampaignId] int NOT NULL,
        [StartedById] int NOT NULL,
        [StartedAt] datetime2 NOT NULL,
        [EndedAt] datetime2 NULL,
        [Status] int NOT NULL DEFAULT 1,
        [CurrentChapterId] int NULL,
        [WelcomeMessage] nvarchar(max) NULL,
        CONSTRAINT [PK_Sessions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Sessions_Campaigns_CampaignId] FOREIGN KEY ([CampaignId])
            REFERENCES [Campaigns] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Sessions_Users_StartedById] FOREIGN KEY ([StartedById])
            REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Sessions_Chapters_CurrentChapterId] FOREIGN KEY ([CurrentChapterId])
            REFERENCES [Chapters] ([Id]) ON DELETE SET NULL
    );
    CREATE INDEX [IX_Sessions_CampaignId] ON [Sessions] ([CampaignId]);
    CREATE INDEX [IX_Sessions_StartedById] ON [Sessions] ([StartedById]);
    CREATE INDEX [IX_Sessions_Status] ON [Sessions] ([Status]);
    CREATE INDEX [IX_Sessions_CurrentChapterId] ON [Sessions] ([CurrentChapterId]);
END");

            // Idempotent: create SessionParticipants table if missing
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[SessionParticipants]', N'U') IS NULL
BEGIN
    CREATE TABLE [SessionParticipants] (
        [Id] int NOT NULL IDENTITY,
        [SessionId] int NOT NULL,
        [WorldCharacterId] int NOT NULL,
        [JoinedAt] datetime2 NOT NULL,
        [Status] int NOT NULL DEFAULT 0,
        CONSTRAINT [PK_SessionParticipants] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SessionParticipants_Sessions_SessionId] FOREIGN KEY ([SessionId])
            REFERENCES [Sessions] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SessionParticipants_WorldCharacters_WorldCharacterId] FOREIGN KEY ([WorldCharacterId])
            REFERENCES [WorldCharacters] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_SessionParticipants_SessionId] ON [SessionParticipants] ([SessionId]);
    CREATE INDEX [IX_SessionParticipants_WorldCharacterId] ON [SessionParticipants] ([WorldCharacterId]);
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Idempotent down: only drop if tables exist
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[SessionParticipants]', N'U') IS NOT NULL
    DROP TABLE [SessionParticipants];
IF OBJECT_ID(N'[Sessions]', N'U') IS NOT NULL
    DROP TABLE [Sessions];");

            migrationBuilder.Sql(@"
IF COL_LENGTH('[dbo].[Characters]', 'IsLocked') IS NOT NULL
BEGIN
    DROP INDEX [IX_Characters_IsLocked] ON [Characters];
    ALTER TABLE [Characters] DROP COLUMN [IsLocked];
END");
        }
    }
}
