using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Cdm.Migrations;

#nullable disable

namespace Cdm.Migrations.Migrations
{
    /// <summary>
    /// Ajoute la validation d'adresse email :
    /// - colonnes [EmailConfirmed] / [EmailConfirmedAt] sur [Users] ;
    /// - table [EmailConfirmationTokens].
    ///
    /// Les comptes existants sont marqués comme NON confirmés (EmailConfirmed = 0) :
    /// ils devront valider leur adresse à leur prochaine connexion.
    /// </summary>
    [DbContext(typeof(MigrationsContext))]
    [Migration("20260719120000_AddEmailConfirmation")]
    public partial class AddEmailConfirmation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF COL_LENGTH('[Users]', 'EmailConfirmed') IS NULL
                    ALTER TABLE [Users] ADD [EmailConfirmed] bit NOT NULL DEFAULT 0;

                IF COL_LENGTH('[Users]', 'EmailConfirmedAt') IS NULL
                    ALTER TABLE [Users] ADD [EmailConfirmedAt] datetime2 NULL;

                IF OBJECT_ID(N'[EmailConfirmationTokens]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [EmailConfirmationTokens] (
                        [Id] int NOT NULL IDENTITY,
                        [Token] nvarchar(200) NOT NULL,
                        [UserId] int NOT NULL,
                        [ExpiresAt] datetime2 NOT NULL,
                        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
                        [UsedAt] datetime2 NULL,
                        CONSTRAINT [PK_EmailConfirmationTokens] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_EmailConfirmationTokens_Users_UserId] FOREIGN KEY ([UserId])
                            REFERENCES [Users] ([Id]) ON DELETE CASCADE
                    );

                    CREATE UNIQUE INDEX [IX_EmailConfirmationTokens_Token]
                        ON [EmailConfirmationTokens] ([Token]);
                    CREATE INDEX [IX_EmailConfirmationTokens_UserId]
                        ON [EmailConfirmationTokens] ([UserId]);
                    CREATE INDEX [IX_EmailConfirmationTokens_ExpiresAt]
                        ON [EmailConfirmationTokens] ([ExpiresAt]);
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[EmailConfirmationTokens]', N'U') IS NOT NULL
                    DROP TABLE [EmailConfirmationTokens];

                IF COL_LENGTH('[Users]', 'EmailConfirmedAt') IS NOT NULL
                    ALTER TABLE [Users] DROP COLUMN [EmailConfirmedAt];

                IF COL_LENGTH('[Users]', 'EmailConfirmed') IS NOT NULL
                BEGIN
                    DECLARE @df sysname;
                    SELECT @df = dc.name FROM sys.default_constraints dc
                        JOIN sys.columns c ON c.default_object_id = dc.object_id
                        WHERE dc.parent_object_id = OBJECT_ID('[Users]') AND c.name = 'EmailConfirmed';
                    IF @df IS NOT NULL EXEC('ALTER TABLE [Users] DROP CONSTRAINT [' + @df + ']');
                    ALTER TABLE [Users] DROP COLUMN [EmailConfirmed];
                END
            ");
        }
    }
}
