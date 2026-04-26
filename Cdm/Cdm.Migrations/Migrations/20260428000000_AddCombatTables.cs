using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Cdm.Migrations;

#nullable disable

namespace Cdm.Migrations.Migrations
{
[DbContext(typeof(MigrationsContext))]
[Migration("20260428000000_AddCombatTables")]
public partial class AddCombatTables : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[Combats]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Combats] (
        [Id]               INT            NOT NULL IDENTITY(1,1),
        [SessionId]        INT            NOT NULL,
        [ChapterId]        INT            NULL,
        [Status]           INT            NOT NULL DEFAULT 0,
        [StartedById]      INT            NOT NULL,
        [CurrentTurnOrder] INT            NOT NULL DEFAULT 0,
        [StartedAt]        DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        [EndedAt]          DATETIME2      NULL,
        [VictorySide]      NVARCHAR(50)   NULL,
        [CreatedAt]        DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_Combats] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Combats_Sessions] FOREIGN KEY ([SessionId])
            REFERENCES [dbo].[Sessions]([Id]) ON DELETE CASCADE
    );
END");

        migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[CombatGroups]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CombatGroups] (
        [Id]           INT           NOT NULL IDENTITY(1,1),
        [CombatId]     INT           NOT NULL,
        [Name]         NVARCHAR(100) NOT NULL,
        [Color]        NVARCHAR(20)  NOT NULL DEFAULT '#6366f1',
        [DisplayOrder] INT           NOT NULL DEFAULT 0,
        CONSTRAINT [PK_CombatGroups] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CombatGroups_Combats] FOREIGN KEY ([CombatId])
            REFERENCES [dbo].[Combats]([Id]) ON DELETE CASCADE
    );
END");

        migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[CombatParticipants]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CombatParticipants] (
        [Id]                INT           NOT NULL IDENTITY(1,1),
        [CombatId]          INT           NOT NULL,
        [GroupId]           INT           NOT NULL,
        [Name]              NVARCHAR(100) NOT NULL,
        [IsPlayerCharacter] BIT           NOT NULL DEFAULT 0,
        [CharacterId]       INT           NULL,
        [NpcId]             INT           NULL,
        [UserId]            INT           NULL,
        [CurrentHp]         INT           NOT NULL DEFAULT 0,
        [MaxHp]             INT           NOT NULL DEFAULT 1,
        [Initiative]        INT           NULL,
        [TurnOrder]         INT           NOT NULL DEFAULT 0,
        [IsActive]          BIT           NOT NULL DEFAULT 1,
        CONSTRAINT [PK_CombatParticipants] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CombatParticipants_Combats] FOREIGN KEY ([CombatId])
            REFERENCES [dbo].[Combats]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CombatParticipants_CombatGroups] FOREIGN KEY ([GroupId])
            REFERENCES [dbo].[CombatGroups]([Id])
    );
END");

        migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[CombatActions]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CombatActions] (
        [Id]                INT            NOT NULL IDENTITY(1,1),
        [CombatId]          INT            NOT NULL,
        [ParticipantName]   NVARCHAR(100)  NOT NULL,
        [ActionType]        NVARCHAR(50)   NOT NULL,
        [Description]       NVARCHAR(500)  NULL,
        [DiceExpression]    NVARCHAR(50)   NULL,
        [DiceResult]        INT            NULL,
        [IsPrivate]         BIT            NOT NULL DEFAULT 0,
        [PerformedByUserId] INT            NULL,
        [CreatedAt]         DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_CombatActions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CombatActions_Combats] FOREIGN KEY ([CombatId])
            REFERENCES [dbo].[Combats]([Id]) ON DELETE CASCADE
    );
END");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[CombatActions]', N'U') IS NOT NULL DROP TABLE [dbo].[CombatActions];");
        migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[CombatParticipants]', N'U') IS NOT NULL DROP TABLE [dbo].[CombatParticipants];");
        migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[CombatGroups]', N'U') IS NOT NULL DROP TABLE [dbo].[CombatGroups];");
        migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[Combats]', N'U') IS NOT NULL DROP TABLE [dbo].[Combats];");
    }
}
}
