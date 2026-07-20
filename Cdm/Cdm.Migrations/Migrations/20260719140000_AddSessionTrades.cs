using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Cdm.Migrations;

#nullable disable

namespace Cdm.Migrations.Migrations
{
    /// <summary>
    /// Ajoute la table [SessionTrades] : les échanges d'objets « théoriques »
    /// proposés en session (MJ→Joueur et Joueur→Joueur), avec leur cycle de vie
    /// (Pending / Accepted / Declined / Cancelled) pour que les propositions en
    /// attente survivent à une reconnexion.
    /// </summary>
    [DbContext(typeof(MigrationsContext))]
    [Migration("20260719140000_AddSessionTrades")]
    public partial class AddSessionTrades : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[SessionTrades]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [SessionTrades] (
                        [Id] int NOT NULL IDENTITY,
                        [SessionId] int NOT NULL,
                        [FromUserId] int NOT NULL,
                        [FromUserName] nvarchar(256) NOT NULL DEFAULT N'',
                        [ToUserId] int NOT NULL,
                        [ToUserName] nvarchar(256) NOT NULL DEFAULT N'',
                        [OfferDescription] nvarchar(1000) NOT NULL DEFAULT N'',
                        [RequestDescription] nvarchar(1000) NOT NULL DEFAULT N'',
                        [Status] int NOT NULL DEFAULT 0,
                        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
                        [RespondedAt] datetime2 NULL,
                        CONSTRAINT [PK_SessionTrades] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_SessionTrades_Sessions_SessionId] FOREIGN KEY ([SessionId])
                            REFERENCES [Sessions] ([Id]) ON DELETE CASCADE
                    );

                    CREATE INDEX [IX_SessionTrades_SessionId] ON [SessionTrades] ([SessionId]);
                    CREATE INDEX [IX_SessionTrades_ToUserId] ON [SessionTrades] ([ToUserId]);
                    CREATE INDEX [IX_SessionTrades_Status] ON [SessionTrades] ([Status]);
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[SessionTrades]', N'U') IS NOT NULL
                    DROP TABLE [SessionTrades];
            ");
        }
    }
}
