using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Cdm.Migrations;

#nullable disable

namespace Cdm.Migrations.Migrations
{
    /// <summary>
    /// Ajoute la table [ChapterImages] : galerie d'images (plans/lieux) attachées à un chapitre
    /// par le MJ, pouvant être montrées aux joueurs en session. Migration idempotente.
    /// </summary>
    [DbContext(typeof(MigrationsContext))]
    [Migration("20260721120000_AddChapterImages")]
    public partial class AddChapterImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[ChapterImages]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [ChapterImages] (
                        [Id] int NOT NULL IDENTITY,
                        [ChapterId] int NOT NULL,
                        [ImageUrl] nvarchar(1000) NOT NULL DEFAULT N'',
                        [Caption] nvarchar(300) NULL,
                        [SortOrder] int NOT NULL DEFAULT 0,
                        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
                        CONSTRAINT [PK_ChapterImages] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_ChapterImages_Chapters_ChapterId] FOREIGN KEY ([ChapterId])
                            REFERENCES [Chapters] ([Id]) ON DELETE CASCADE
                    );

                    CREATE INDEX [IX_ChapterImages_ChapterId] ON [ChapterImages] ([ChapterId]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF OBJECT_ID(N'[ChapterImages]', N'U') IS NOT NULL DROP TABLE [ChapterImages];");
        }
    }
}
