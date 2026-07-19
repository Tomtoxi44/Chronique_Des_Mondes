using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Cdm.Migrations;

#nullable disable

namespace Cdm.Migrations.Migrations
{
    /// <summary>
    /// Corrige une incohérence de modèle : les tables SessionMessages et SessionDiceRolls
    /// portaient une colonne [ChapterId] avec une clé étrangère vers [Chapters], alors que
    /// le code y écrivait en réalité un identifiant de SESSION (les composants Blazor
    /// envoient le SessionId au hub SignalR).
    ///
    /// Conséquence : l'insertion échouait dès qu'aucun chapitre ne portait le même
    /// identifiant que la session — ce qui rendait le chat et l'historique des dés muets.
    ///
    /// Cette migration renomme la colonne en [SessionId] et repointe la clé étrangère
    /// vers [Sessions]. Les lignes dont la valeur ne correspond à aucune session existante
    /// sont supprimées : elles sont sémantiquement ambiguës (des identifiants de chapitre
    /// enregistrés par erreur) et empêcheraient la création de la nouvelle contrainte.
    /// </summary>
    [DbContext(typeof(MigrationsContext))]
    [Migration("20260718120000_RenameSessionChapterIdToSessionId")]
    public partial class RenameSessionChapterIdToSessionId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- ============================================================
                -- SessionMessages : [ChapterId] -> [SessionId]
                -- ============================================================
                IF OBJECT_ID(N'[SessionMessages]', N'U') IS NOT NULL
                   AND COL_LENGTH('[SessionMessages]', 'ChapterId') IS NOT NULL
                BEGIN
                    -- Lignes non rattachables à une session existante : on les écarte.
                    DELETE FROM [SessionMessages]
                    WHERE [ChapterId] NOT IN (SELECT [Id] FROM [Sessions]);

                    IF OBJECT_ID(N'FK_SessionMessages_Chapters_ChapterId', N'F') IS NOT NULL
                        ALTER TABLE [SessionMessages] DROP CONSTRAINT [FK_SessionMessages_Chapters_ChapterId];

                    IF EXISTS (SELECT 1 FROM sys.indexes
                               WHERE name = 'IX_SessionMessages_ChapterId'
                                 AND object_id = OBJECT_ID(N'[SessionMessages]'))
                        DROP INDEX [IX_SessionMessages_ChapterId] ON [SessionMessages];

                    EXEC sp_rename '[SessionMessages].[ChapterId]', 'SessionId', 'COLUMN';

                    CREATE INDEX [IX_SessionMessages_SessionId]
                        ON [SessionMessages] ([SessionId]);

                    ALTER TABLE [SessionMessages]
                        ADD CONSTRAINT [FK_SessionMessages_Sessions_SessionId]
                        FOREIGN KEY ([SessionId]) REFERENCES [Sessions] ([Id]) ON DELETE CASCADE;
                END

                -- ============================================================
                -- SessionDiceRolls : [ChapterId] -> [SessionId]
                -- ============================================================
                IF OBJECT_ID(N'[SessionDiceRolls]', N'U') IS NOT NULL
                   AND COL_LENGTH('[SessionDiceRolls]', 'ChapterId') IS NOT NULL
                BEGIN
                    DELETE FROM [SessionDiceRolls]
                    WHERE [ChapterId] NOT IN (SELECT [Id] FROM [Sessions]);

                    IF OBJECT_ID(N'FK_SessionDiceRolls_Chapters_ChapterId', N'F') IS NOT NULL
                        ALTER TABLE [SessionDiceRolls] DROP CONSTRAINT [FK_SessionDiceRolls_Chapters_ChapterId];

                    IF EXISTS (SELECT 1 FROM sys.indexes
                               WHERE name = 'IX_SessionDiceRolls_ChapterId'
                                 AND object_id = OBJECT_ID(N'[SessionDiceRolls]'))
                        DROP INDEX [IX_SessionDiceRolls_ChapterId] ON [SessionDiceRolls];

                    EXEC sp_rename '[SessionDiceRolls].[ChapterId]', 'SessionId', 'COLUMN';

                    CREATE INDEX [IX_SessionDiceRolls_SessionId]
                        ON [SessionDiceRolls] ([SessionId]);

                    ALTER TABLE [SessionDiceRolls]
                        ADD CONSTRAINT [FK_SessionDiceRolls_Sessions_SessionId]
                        FOREIGN KEY ([SessionId]) REFERENCES [Sessions] ([Id]) ON DELETE CASCADE;
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[SessionMessages]', N'U') IS NOT NULL
                   AND COL_LENGTH('[SessionMessages]', 'SessionId') IS NOT NULL
                BEGIN
                    IF OBJECT_ID(N'FK_SessionMessages_Sessions_SessionId', N'F') IS NOT NULL
                        ALTER TABLE [SessionMessages] DROP CONSTRAINT [FK_SessionMessages_Sessions_SessionId];

                    IF EXISTS (SELECT 1 FROM sys.indexes
                               WHERE name = 'IX_SessionMessages_SessionId'
                                 AND object_id = OBJECT_ID(N'[SessionMessages]'))
                        DROP INDEX [IX_SessionMessages_SessionId] ON [SessionMessages];

                    EXEC sp_rename '[SessionMessages].[SessionId]', 'ChapterId', 'COLUMN';

                    CREATE INDEX [IX_SessionMessages_ChapterId]
                        ON [SessionMessages] ([ChapterId]);
                END

                IF OBJECT_ID(N'[SessionDiceRolls]', N'U') IS NOT NULL
                   AND COL_LENGTH('[SessionDiceRolls]', 'SessionId') IS NOT NULL
                BEGIN
                    IF OBJECT_ID(N'FK_SessionDiceRolls_Sessions_SessionId', N'F') IS NOT NULL
                        ALTER TABLE [SessionDiceRolls] DROP CONSTRAINT [FK_SessionDiceRolls_Sessions_SessionId];

                    IF EXISTS (SELECT 1 FROM sys.indexes
                               WHERE name = 'IX_SessionDiceRolls_SessionId'
                                 AND object_id = OBJECT_ID(N'[SessionDiceRolls]'))
                        DROP INDEX [IX_SessionDiceRolls_SessionId] ON [SessionDiceRolls];

                    EXEC sp_rename '[SessionDiceRolls].[SessionId]', 'ChapterId', 'COLUMN';

                    CREATE INDEX [IX_SessionDiceRolls_ChapterId]
                        ON [SessionDiceRolls] ([ChapterId]);
                END
            ");

            // NB : la clé étrangère vers [Chapters] n'est volontairement pas recréée —
            // c'est précisément elle qui était erronée.
        }
    }
}
