using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Cdm.Migrations;

#nullable disable

namespace Cdm.Migrations.Migrations
{
    [DbContext(typeof(MigrationsContext))]
    [Migration("20260418150000_CreateNotificationsTable")]
    public partial class CreateNotificationsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[Notifications]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [Notifications] (
                        [Id] int NOT NULL IDENTITY,
                        [UserId] int NOT NULL,
                        [Type] int NOT NULL,
                        [Title] nvarchar(200) NOT NULL,
                        [Message] nvarchar(max) NOT NULL,
                        [RelatedEntityId] int NULL,
                        [RelatedEntityType] nvarchar(50) NULL,
                        [ActionUrl] nvarchar(500) NULL,
                        [IsRead] bit NOT NULL DEFAULT CAST(0 AS bit),
                        [ReadAt] datetime2 NULL,
                        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
                        [SentBy] int NULL,
                        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY ([UserId])
                            REFERENCES [Users] ([Id]) ON DELETE CASCADE,
                        CONSTRAINT [FK_Notifications_Users_SentBy] FOREIGN KEY ([SentBy])
                            REFERENCES [Users] ([Id]) ON DELETE NO ACTION
                    );

                    CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
                    CREATE INDEX [IX_Notifications_UserId_IsRead] ON [Notifications] ([UserId], [IsRead]);
                    CREATE INDEX [IX_Notifications_CreatedAt] ON [Notifications] ([CreatedAt]);
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[Notifications]', N'U') IS NOT NULL DROP TABLE [Notifications];
            ");
        }
    }
}
