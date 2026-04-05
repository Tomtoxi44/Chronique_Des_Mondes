using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cdm.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignInviteAndStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InviteToken",
                table: "Campaigns",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Campaigns",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InviteToken",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Campaigns");
        }
    }
}
