// -----------------------------------------------------------------------
// <copyright file="20260422100000_AddDndInventoryAndCharacterSpells.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Migrations.Migrations;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
/// Idempotent migration: creates DndInventoryItems and DndCharacterSpells tables.
/// </summary>
[DbContext(typeof(Cdm.Data.Common.AppDbContext))]
[Migration("20260422100000_AddDndInventoryAndCharacterSpells")]
public class AddDndInventoryAndCharacterSpells : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // ── DndInventoryItems ────────────────────────────────────────────────
        migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[DndInventoryItems]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DndInventoryItems] (
        [Id]               int NOT NULL IDENTITY PRIMARY KEY,
        [WorldCharacterId] int NOT NULL,
        [Name]             nvarchar(200) NOT NULL,
        [Category]         nvarchar(50) NOT NULL,
        [Quantity]         int NOT NULL DEFAULT 1,
        [AttackBonus]      int NULL,
        [DamageDice]       nvarchar(20) NULL,
        [DamageType]       nvarchar(50) NULL,
        [Notes]            nvarchar(500) NULL,
        [DndItemId]        int NULL,
        [CreatedAt]        datetime2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [FK_DndInventoryItems_WorldCharacters]
            FOREIGN KEY ([WorldCharacterId]) REFERENCES [dbo].[WorldCharacters]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_DndInventoryItems_DndItems]
            FOREIGN KEY ([DndItemId]) REFERENCES [dbo].[DndItems]([Id]) ON DELETE SET NULL
    );
    CREATE INDEX [IX_DndInventoryItems_WorldCharacterId] ON [dbo].[DndInventoryItems] ([WorldCharacterId]);
END");

        // ── DndCharacterSpells ───────────────────────────────────────────────
        migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[DndCharacterSpells]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DndCharacterSpells] (
        [Id]               int NOT NULL IDENTITY PRIMARY KEY,
        [WorldCharacterId] int NOT NULL,
        [Name]             nvarchar(200) NOT NULL,
        [Level]            int NOT NULL DEFAULT 0,
        [School]           nvarchar(50) NULL,
        [Description]      nvarchar(max) NULL,
        [DamageDice]       nvarchar(20) NULL,
        [DamageType]       nvarchar(50) NULL,
        [IsPrepared]       bit NOT NULL DEFAULT 0,
        [DndSpellId]       int NULL,
        [CreatedAt]        datetime2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [FK_DndCharacterSpells_WorldCharacters]
            FOREIGN KEY ([WorldCharacterId]) REFERENCES [dbo].[WorldCharacters]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_DndCharacterSpells_DndSpells]
            FOREIGN KEY ([DndSpellId]) REFERENCES [dbo].[DndSpells]([Id]) ON DELETE SET NULL
    );
    CREATE INDEX [IX_DndCharacterSpells_WorldCharacterId] ON [dbo].[DndCharacterSpells] ([WorldCharacterId]);
END");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP TABLE IF EXISTS [dbo].[DndInventoryItems];");
        migrationBuilder.Sql("DROP TABLE IF EXISTS [dbo].[DndCharacterSpells];");
    }
}
