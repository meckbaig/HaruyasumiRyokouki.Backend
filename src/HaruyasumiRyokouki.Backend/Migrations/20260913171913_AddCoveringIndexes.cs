using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HaruyasumiRyokouki.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCoveringIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_tag_labels_primary_per_language",
                table: "tag_translations");

            migrationBuilder.CreateIndex(
                name: "ix_tags_id",
                table: "tags",
                column: "id")
                .Annotation("Npgsql:IndexInclude", new[] { "slug" });

            migrationBuilder.CreateIndex(
                name: "ux_tag_labels_primary_per_language",
                table: "tag_translations",
                columns: new[] { "tag_id", "language_code" },
                unique: true,
                filter: "is_primary")
                .Annotation("Npgsql:IndexInclude", new[] { "id", "text" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tags_id",
                table: "tags");

            migrationBuilder.DropIndex(
                name: "ux_tag_labels_primary_per_language",
                table: "tag_translations");

            migrationBuilder.CreateIndex(
                name: "ux_tag_labels_primary_per_language",
                table: "tag_translations",
                columns: new[] { "tag_id", "language_code" },
                unique: true,
                filter: "is_primary");
        }
    }
}
