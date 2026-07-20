using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Cdm.Migrations;

#nullable disable

namespace Cdm.Migrations.Migrations
{
    /// <summary>
    /// Ajoute la colonne [DexterityModifier] à [CombatParticipants] : le modificateur
    /// de Dextérité utilisé pour l'initiative automatique (1d20 + mod. DEX).
    /// </summary>
    [DbContext(typeof(MigrationsContext))]
    [Migration("20260719160000_AddCombatParticipantDexModifier")]
    public partial class AddCombatParticipantDexModifier : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF COL_LENGTH('[CombatParticipants]', 'DexterityModifier') IS NULL
                    ALTER TABLE [CombatParticipants] ADD [DexterityModifier] int NOT NULL DEFAULT 0;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF COL_LENGTH('[CombatParticipants]', 'DexterityModifier') IS NOT NULL
                BEGIN
                    DECLARE @df sysname;
                    SELECT @df = dc.name FROM sys.default_constraints dc
                        JOIN sys.columns c ON c.default_object_id = dc.object_id
                        WHERE dc.parent_object_id = OBJECT_ID('[CombatParticipants]') AND c.name = 'DexterityModifier';
                    IF @df IS NOT NULL EXEC('ALTER TABLE [CombatParticipants] DROP CONSTRAINT [' + @df + ']');
                    ALTER TABLE [CombatParticipants] DROP COLUMN [DexterityModifier];
                END
            ");
        }
    }
}
