using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Cdm.Migrations;

#nullable disable

namespace Cdm.Migrations.Migrations
{
    [DbContext(typeof(MigrationsContext))]
    [Migration("20260418160000_AddGameMasterRoleToExistingUsers")]
    public class AddGameMasterRoleToExistingUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure roles are seeded (idempotent — handles local DBs built from old migrations without seed)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Id] = 1)
                    INSERT INTO [Roles] ([Id], [CreatedAt], [Name]) VALUES (1, '2025-11-04 00:00:00', 'Player');
                IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Id] = 2)
                    INSERT INTO [Roles] ([Id], [CreatedAt], [Name]) VALUES (2, '2025-11-04 00:00:00', 'GameMaster');
                IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Id] = 3)
                    INSERT INTO [Roles] ([Id], [CreatedAt], [Name]) VALUES (3, '2025-11-04 00:00:00', 'Admin');
            ");

            // Add GameMaster role to all existing users who have Player but not GameMaster
            migrationBuilder.Sql(@"
                INSERT INTO [UserRoles] ([UserId], [RoleId], [AssignedAt])
                SELECT ur.[UserId], 2, GETUTCDATE()
                FROM [UserRoles] ur
                WHERE ur.[RoleId] = 1
                AND NOT EXISTS (
                    SELECT 1 FROM [UserRoles] ur2
                    WHERE ur2.[UserId] = ur.[UserId] AND ur2.[RoleId] = 2
                );
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove GameMaster from users who also have Player (best-effort rollback)
            migrationBuilder.Sql(@"
                DELETE FROM [UserRoles]
                WHERE [RoleId] = 2
                AND EXISTS (
                    SELECT 1 FROM [UserRoles] ur2
                    WHERE ur2.[UserId] = [UserRoles].[UserId] AND ur2.[RoleId] = 1
                );
            ");
        }
    }
}
