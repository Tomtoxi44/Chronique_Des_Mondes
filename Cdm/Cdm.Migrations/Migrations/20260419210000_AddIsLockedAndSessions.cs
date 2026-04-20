using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cdm.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddIsLockedAndSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty.
            // IsLocked + IX_Characters_IsLocked were already created by InitialCreate (20260328182844).
            // Sessions + SessionParticipants are created idempotently by 20260420000000_EnsureSessionTablesAndIsLocked.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
