using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Cdm.Migrations;

#nullable disable

namespace Cdm.Migrations.Migrations
{
    /// <summary>
    /// Ajoute les colonnes de défense à [CombatParticipants] pour la résolution serveur
    /// des attaques : [ArmorClass] (classe d'armure), [Resistances] et [Vulnerabilities]
    /// (types de dégâts, séparés par des virgules).
    /// </summary>
    [DbContext(typeof(MigrationsContext))]
    [Migration("20260719170000_AddCombatParticipantDefense")]
    public partial class AddCombatParticipantDefense : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF COL_LENGTH('[CombatParticipants]', 'ArmorClass') IS NULL
                    ALTER TABLE [CombatParticipants] ADD [ArmorClass] int NOT NULL DEFAULT 10;

                IF COL_LENGTH('[CombatParticipants]', 'Resistances') IS NULL
                    ALTER TABLE [CombatParticipants] ADD [Resistances] nvarchar(400) NULL;

                IF COL_LENGTH('[CombatParticipants]', 'Vulnerabilities') IS NULL
                    ALTER TABLE [CombatParticipants] ADD [Vulnerabilities] nvarchar(400) NULL;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF COL_LENGTH('[CombatParticipants]', 'Vulnerabilities') IS NOT NULL
                    ALTER TABLE [CombatParticipants] DROP COLUMN [Vulnerabilities];

                IF COL_LENGTH('[CombatParticipants]', 'Resistances') IS NOT NULL
                    ALTER TABLE [CombatParticipants] DROP COLUMN [Resistances];

                IF COL_LENGTH('[CombatParticipants]', 'ArmorClass') IS NOT NULL
                BEGIN
                    DECLARE @df sysname;
                    SELECT @df = dc.name FROM sys.default_constraints dc
                        JOIN sys.columns c ON c.default_object_id = dc.object_id
                        WHERE dc.parent_object_id = OBJECT_ID('[CombatParticipants]') AND c.name = 'ArmorClass';
                    IF @df IS NOT NULL EXEC('ALTER TABLE [CombatParticipants] DROP CONSTRAINT [' + @df + ']');
                    ALTER TABLE [CombatParticipants] DROP COLUMN [ArmorClass];
                END
            ");
        }
    }
}
