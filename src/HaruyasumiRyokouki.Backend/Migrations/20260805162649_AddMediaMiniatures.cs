using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HaruyasumiRyokouki.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaMiniatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "miniature",
                table: "media_files",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "miniature",
                table: "media_files");
        }
    }
}
