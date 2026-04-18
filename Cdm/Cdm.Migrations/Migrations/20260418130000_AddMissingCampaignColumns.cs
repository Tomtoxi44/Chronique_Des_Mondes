using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Cdm.Migrations;

#nullable disable

namespace Cdm.Migrations.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(MigrationsContext))]
    [Migration("20260418130000_AddMissingCampaignColumns")]
    public partial class AddMissingCampaignColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = 'Campaigns' AND COLUMN_NAME = 'InviteToken'
                )
                BEGIN
                    ALTER TABLE [Campaigns] ADD [InviteToken] nvarchar(100) NULL;
                END

                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = 'Campaigns' AND COLUMN_NAME = 'Status'
                )
                BEGIN
                    ALTER TABLE [Campaigns] ADD [Status] int NOT NULL DEFAULT 0;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = 'Campaigns' AND COLUMN_NAME = 'InviteToken'
                )
                BEGIN
                    ALTER TABLE [Campaigns] DROP COLUMN [InviteToken];
                END

                IF EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = 'Campaigns' AND COLUMN_NAME = 'Status'
                )
                BEGIN
                    ALTER TABLE [Campaigns] DROP COLUMN [Status];
                END
            ");
        }
    }
}
