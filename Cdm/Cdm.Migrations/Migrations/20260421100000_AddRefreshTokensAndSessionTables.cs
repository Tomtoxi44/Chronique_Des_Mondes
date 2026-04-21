using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Cdm.Migrations;

#nullable disable

namespace Cdm.Migrations.Migrations
{
    [DbContext(typeof(MigrationsContext))]
    [Migration("20260421100000_AddRefreshTokensAndSessionTables")]
    public partial class AddRefreshTokensAndSessionTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[RefreshTokens]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [RefreshTokens] (
                        [Id] int NOT NULL IDENTITY,
                        [Token] nvarchar(500) NOT NULL,
                        [UserId] int NOT NULL,
                        [ExpiresAt] datetime2 NOT NULL,
                        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
                        [RevokedAt] datetime2 NULL,
                        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_RefreshTokens_Users_UserId] FOREIGN KEY ([UserId])
                            REFERENCES [Users] ([Id]) ON DELETE CASCADE
                    );

                    CREATE UNIQUE INDEX [IX_RefreshTokens_Token] ON [RefreshTokens] ([Token]);
                    CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);
                    CREATE INDEX [IX_RefreshTokens_ExpiresAt] ON [RefreshTokens] ([ExpiresAt]);
                END

                IF OBJECT_ID(N'[SessionMessages]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [SessionMessages] (
                        [Id] int NOT NULL IDENTITY,
                        [ChapterId] int NOT NULL,
                        [UserId] int NOT NULL,
                        [UserName] nvarchar(200) NOT NULL,
                        [Message] nvarchar(max) NOT NULL,
                        [SentAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
                        CONSTRAINT [PK_SessionMessages] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_SessionMessages_Chapters_ChapterId] FOREIGN KEY ([ChapterId])
                            REFERENCES [Chapters] ([Id]) ON DELETE CASCADE,
                        CONSTRAINT [FK_SessionMessages_Users_UserId] FOREIGN KEY ([UserId])
                            REFERENCES [Users] ([Id]) ON DELETE NO ACTION
                    );

                    CREATE INDEX [IX_SessionMessages_ChapterId] ON [SessionMessages] ([ChapterId]);
                    CREATE INDEX [IX_SessionMessages_UserId] ON [SessionMessages] ([UserId]);
                    CREATE INDEX [IX_SessionMessages_SentAt] ON [SessionMessages] ([SentAt]);
                END

                IF OBJECT_ID(N'[SessionDiceRolls]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [SessionDiceRolls] (
                        [Id] int NOT NULL IDENTITY,
                        [ChapterId] int NOT NULL,
                        [UserId] int NOT NULL,
                        [UserName] nvarchar(200) NOT NULL,
                        [DiceType] nvarchar(20) NOT NULL,
                        [Count] int NOT NULL,
                        [Results] nvarchar(500) NOT NULL,
                        [Modifier] int NOT NULL DEFAULT 0,
                        [Total] int NOT NULL,
                        [Reason] nvarchar(500) NULL,
                        [RolledAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
                        CONSTRAINT [PK_SessionDiceRolls] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_SessionDiceRolls_Chapters_ChapterId] FOREIGN KEY ([ChapterId])
                            REFERENCES [Chapters] ([Id]) ON DELETE CASCADE,
                        CONSTRAINT [FK_SessionDiceRolls_Users_UserId] FOREIGN KEY ([UserId])
                            REFERENCES [Users] ([Id]) ON DELETE NO ACTION
                    );

                    CREATE INDEX [IX_SessionDiceRolls_ChapterId] ON [SessionDiceRolls] ([ChapterId]);
                    CREATE INDEX [IX_SessionDiceRolls_UserId] ON [SessionDiceRolls] ([UserId]);
                    CREATE INDEX [IX_SessionDiceRolls_DiceType] ON [SessionDiceRolls] ([DiceType]);
                    CREATE INDEX [IX_SessionDiceRolls_RolledAt] ON [SessionDiceRolls] ([RolledAt]);
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[SessionDiceRolls]', N'U') IS NOT NULL DROP TABLE [SessionDiceRolls];
                IF OBJECT_ID(N'[SessionMessages]', N'U') IS NOT NULL DROP TABLE [SessionMessages];
                IF OBJECT_ID(N'[RefreshTokens]', N'U') IS NOT NULL DROP TABLE [RefreshTokens];
            ");
        }
    }
}
