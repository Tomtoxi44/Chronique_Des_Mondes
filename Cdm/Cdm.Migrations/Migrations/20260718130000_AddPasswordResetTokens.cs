using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Cdm.Migrations;

#nullable disable

namespace Cdm.Migrations.Migrations
{
    /// <summary>
    /// Ajoute la table des jetons de réinitialisation de mot de passe
    /// (parcours « mot de passe oublié »).
    /// </summary>
    [DbContext(typeof(MigrationsContext))]
    [Migration("20260718130000_AddPasswordResetTokens")]
    public partial class AddPasswordResetTokens : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[PasswordResetTokens]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [PasswordResetTokens] (
                        [Id] int NOT NULL IDENTITY,
                        [Token] nvarchar(200) NOT NULL,
                        [UserId] int NOT NULL,
                        [ExpiresAt] datetime2 NOT NULL,
                        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
                        [UsedAt] datetime2 NULL,
                        CONSTRAINT [PK_PasswordResetTokens] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_PasswordResetTokens_Users_UserId] FOREIGN KEY ([UserId])
                            REFERENCES [Users] ([Id]) ON DELETE CASCADE
                    );

                    CREATE UNIQUE INDEX [IX_PasswordResetTokens_Token]
                        ON [PasswordResetTokens] ([Token]);
                    CREATE INDEX [IX_PasswordResetTokens_UserId]
                        ON [PasswordResetTokens] ([UserId]);
                    CREATE INDEX [IX_PasswordResetTokens_ExpiresAt]
                        ON [PasswordResetTokens] ([ExpiresAt]);
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[PasswordResetTokens]', N'U') IS NOT NULL
                    DROP TABLE [PasswordResetTokens];
            ");
        }
    }
}
