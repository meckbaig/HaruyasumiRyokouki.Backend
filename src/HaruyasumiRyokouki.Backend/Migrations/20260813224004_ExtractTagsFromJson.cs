using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HaruyasumiRyokouki.Backend.Migrations
{
    /// <inheritdoc />
    public partial class ExtractTagsFromJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_media_translations_tags",
                table: "media_translations");

            migrationBuilder.DropColumn(
                name: "tags",
                table: "media_translations");

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    slug = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "media_file_tag",
                columns: table => new
                {
                    media_id = table.Column<int>(type: "integer", nullable: false),
                    tags_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_media_file_tag", x => new { x.media_id, x.tags_id });
                    table.ForeignKey(
                        name: "fk_media_file_tag_media_files_media_id",
                        column: x => x.media_id,
                        principalTable: "media_files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_media_file_tag_tags_tags_id",
                        column: x => x.tags_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tag_translations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tag_id = table.Column<int>(type: "integer", nullable: false),
                    language_code = table.Column<string>(type: "text", nullable: false),
                    text = table.Column<string>(type: "text", nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tag_translations", x => x.id);
                    table.ForeignKey(
                        name: "fk_tag_translations_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_media_file_tag_tags_id",
                table: "media_file_tag",
                column: "tags_id");

            migrationBuilder.CreateIndex(
                name: "ix_tag_translations_tag_id_text",
                table: "tag_translations",
                columns: new[] { "tag_id", "text" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tag_translations_text",
                table: "tag_translations",
                column: "text")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ux_tag_labels_primary_per_language",
                table: "tag_translations",
                columns: new[] { "tag_id", "language_code" },
                unique: true,
                filter: "is_primary");

            migrationBuilder.CreateIndex(
                name: "ix_tags_slug",
                table: "tags",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "media_file_tag");

            migrationBuilder.DropTable(
                name: "tag_translations");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.AddColumn<string[]>(
                name: "tags",
                table: "media_translations",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.CreateIndex(
                name: "ix_media_translations_tags",
                table: "media_translations",
                column: "tags")
                .Annotation("Npgsql:IndexMethod", "gin");
        }
    }
}
