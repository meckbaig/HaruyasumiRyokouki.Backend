using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HaruyasumiRyokouki.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaSourceAndExternalUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "external_url",
                table: "media_files",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "media_files",
                type: "text",
                nullable: false,
                defaultValue: "Local");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "external_url",
                table: "media_files");

            migrationBuilder.DropColumn(
                name: "source",
                table: "media_files");
        }
    }
}
