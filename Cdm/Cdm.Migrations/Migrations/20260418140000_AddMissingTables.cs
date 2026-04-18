using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Cdm.Migrations;

#nullable disable

namespace Cdm.Migrations.Migrations
{
    [DbContext(typeof(MigrationsContext))]
    [Migration("20260418140000_AddMissingTables")]
    public partial class AddMissingTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Chapters table
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[Chapters]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [Chapters] (
                        [Id] int NOT NULL IDENTITY,
                        [CampaignId] int NOT NULL,
                        [ChapterNumber] int NOT NULL,
                        [Title] nvarchar(200) NOT NULL,
                        [Content] nvarchar(max) NULL,
                        [IsCompleted] bit NOT NULL DEFAULT CAST(0 AS bit),
                        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
                        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
                        [UpdatedAt] datetime2 NULL,
                        [CompletedAt] datetime2 NULL,
                        CONSTRAINT [PK_Chapters] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_Chapters_Campaigns_CampaignId] FOREIGN KEY ([CampaignId])
                            REFERENCES [Campaigns] ([Id]) ON DELETE NO ACTION
                    );

                    CREATE INDEX [IX_Chapters_CampaignId] ON [Chapters] ([CampaignId]);
                    CREATE INDEX [IX_Chapters_CampaignId_ChapterNumber] ON [Chapters] ([CampaignId], [ChapterNumber]);
                    CREATE INDEX [IX_Chapters_IsCompleted] ON [Chapters] ([IsCompleted]);
                END
            ");

            // Achievements table
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[Achievements]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [Achievements] (
                        [Id] int NOT NULL IDENTITY,
                        [Name] nvarchar(200) NOT NULL,
                        [Description] nvarchar(max) NOT NULL,
                        [Level] int NOT NULL,
                        [WorldId] int NULL,
                        [CampaignId] int NULL,
                        [ChapterId] int NULL,
                        [Rarity] int NOT NULL DEFAULT 0,
                        [IconUrl] nvarchar(500) NULL,
                        [RewardDescription] nvarchar(500) NULL,
                        [IsAutomatic] bit NOT NULL DEFAULT CAST(0 AS bit),
                        [AutomaticCondition] nvarchar(max) NULL,
                        [IsSecret] bit NOT NULL DEFAULT CAST(0 AS bit),
                        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
                        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
                        [UpdatedAt] datetime2 NULL,
                        [CreatedBy] int NOT NULL,
                        CONSTRAINT [PK_Achievements] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_Achievements_Campaigns_CampaignId] FOREIGN KEY ([CampaignId])
                            REFERENCES [Campaigns] ([Id]) ON DELETE NO ACTION,
                        CONSTRAINT [FK_Achievements_Chapters_ChapterId] FOREIGN KEY ([ChapterId])
                            REFERENCES [Chapters] ([Id]) ON DELETE NO ACTION,
                        CONSTRAINT [FK_Achievements_Users_CreatedBy] FOREIGN KEY ([CreatedBy])
                            REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
                        CONSTRAINT [FK_Achievements_Worlds_WorldId] FOREIGN KEY ([WorldId])
                            REFERENCES [Worlds] ([Id]) ON DELETE NO ACTION
                    );

                    CREATE INDEX [IX_Achievements_CampaignId] ON [Achievements] ([CampaignId]);
                    CREATE INDEX [IX_Achievements_ChapterId] ON [Achievements] ([ChapterId]);
                    CREATE INDEX [IX_Achievements_CreatedBy] ON [Achievements] ([CreatedBy]);
                    CREATE INDEX [IX_Achievements_IsActive] ON [Achievements] ([IsActive]);
                    CREATE INDEX [IX_Achievements_Level] ON [Achievements] ([Level]);
                    CREATE INDEX [IX_Achievements_Rarity] ON [Achievements] ([Rarity]);
                    CREATE INDEX [IX_Achievements_WorldId] ON [Achievements] ([WorldId]);
                END
            ");

            // Events table
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[Events]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [Events] (
                        [Id] int NOT NULL IDENTITY,
                        [Name] nvarchar(200) NOT NULL,
                        [Description] nvarchar(max) NULL,
                        [Level] int NOT NULL,
                        [WorldId] int NULL,
                        [CampaignId] int NULL,
                        [ChapterId] int NULL,
                        [EffectType] int NOT NULL,
                        [TargetStat] nvarchar(100) NULL,
                        [ModifierValue] int NULL,
                        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
                        [IsPermanent] bit NOT NULL DEFAULT CAST(1 AS bit),
                        [ExpiresAt] datetime2 NULL,
                        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
                        [UpdatedAt] datetime2 NULL,
                        [CreatedBy] int NOT NULL,
                        CONSTRAINT [PK_Events] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_Events_Campaigns_CampaignId] FOREIGN KEY ([CampaignId])
                            REFERENCES [Campaigns] ([Id]) ON DELETE NO ACTION,
                        CONSTRAINT [FK_Events_Chapters_ChapterId] FOREIGN KEY ([ChapterId])
                            REFERENCES [Chapters] ([Id]) ON DELETE NO ACTION,
                        CONSTRAINT [FK_Events_Users_CreatedBy] FOREIGN KEY ([CreatedBy])
                            REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
                        CONSTRAINT [FK_Events_Worlds_WorldId] FOREIGN KEY ([WorldId])
                            REFERENCES [Worlds] ([Id]) ON DELETE NO ACTION
                    );

                    CREATE INDEX [IX_Events_CampaignId] ON [Events] ([CampaignId]);
                    CREATE INDEX [IX_Events_ChapterId] ON [Events] ([ChapterId]);
                    CREATE INDEX [IX_Events_CreatedBy] ON [Events] ([CreatedBy]);
                    CREATE INDEX [IX_Events_IsActive] ON [Events] ([IsActive]);
                    CREATE INDEX [IX_Events_Level] ON [Events] ([Level]);
                    CREATE INDEX [IX_Events_WorldId] ON [Events] ([WorldId]);
                END
            ");

            // UserAchievements table
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[UserAchievements]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [UserAchievements] (
                        [Id] int NOT NULL IDENTITY,
                        [UserId] int NOT NULL,
                        [AchievementId] int NOT NULL,
                        [UnlockedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
                        [IsManuallyAwarded] bit NOT NULL DEFAULT CAST(0 AS bit),
                        [AwardedBy] int NULL,
                        [AwardMessage] nvarchar(500) NULL,
                        CONSTRAINT [PK_UserAchievements] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_UserAchievements_Achievements_AchievementId] FOREIGN KEY ([AchievementId])
                            REFERENCES [Achievements] ([Id]) ON DELETE CASCADE,
                        CONSTRAINT [FK_UserAchievements_Users_AwardedBy] FOREIGN KEY ([AwardedBy])
                            REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
                        CONSTRAINT [FK_UserAchievements_Users_UserId] FOREIGN KEY ([UserId])
                            REFERENCES [Users] ([Id]) ON DELETE CASCADE
                    );

                    CREATE INDEX [IX_UserAchievements_AchievementId] ON [UserAchievements] ([AchievementId]);
                    CREATE INDEX [IX_UserAchievements_AwardedBy] ON [UserAchievements] ([AwardedBy]);
                    CREATE INDEX [IX_UserAchievements_UnlockedAt] ON [UserAchievements] ([UnlockedAt]);
                    CREATE INDEX [IX_UserAchievements_UserId] ON [UserAchievements] ([UserId]);
                    CREATE UNIQUE INDEX [IX_UserAchievements_UserId_AchievementId] ON [UserAchievements] ([UserId], [AchievementId]);
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[UserAchievements]', N'U') IS NOT NULL DROP TABLE [UserAchievements];
                IF OBJECT_ID(N'[Events]', N'U') IS NOT NULL DROP TABLE [Events];
                IF OBJECT_ID(N'[Achievements]', N'U') IS NOT NULL DROP TABLE [Achievements];
                IF OBJECT_ID(N'[Chapters]', N'U') IS NOT NULL DROP TABLE [Chapters];
            ");
        }
    }
}
