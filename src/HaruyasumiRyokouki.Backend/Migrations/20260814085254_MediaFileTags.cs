using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HaruyasumiRyokouki.Backend.Migrations
{
    /// <inheritdoc />
    public partial class MediaFileTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_media_file_tag_tags_tags_id",
                table: "media_file_tag");

            migrationBuilder.RenameColumn(
                name: "tags_id",
                table: "media_file_tag",
                newName: "tag_id");

            migrationBuilder.RenameIndex(
                name: "ix_media_file_tag_tags_id",
                table: "media_file_tag",
                newName: "ix_media_file_tag_tag_id");

            migrationBuilder.AddForeignKey(
                name: "fk_media_file_tag_tags_tag_id",
                table: "media_file_tag",
                column: "tag_id",
                principalTable: "tags",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_media_file_tag_tags_tag_id",
                table: "media_file_tag");

            migrationBuilder.RenameColumn(
                name: "tag_id",
                table: "media_file_tag",
                newName: "tags_id");

            migrationBuilder.RenameIndex(
                name: "ix_media_file_tag_tag_id",
                table: "media_file_tag",
                newName: "ix_media_file_tag_tags_id");

            migrationBuilder.AddForeignKey(
                name: "fk_media_file_tag_tags_tags_id",
                table: "media_file_tag",
                column: "tags_id",
                principalTable: "tags",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
