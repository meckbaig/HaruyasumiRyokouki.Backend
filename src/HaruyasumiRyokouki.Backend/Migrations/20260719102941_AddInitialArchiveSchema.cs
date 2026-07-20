using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HaruyasumiRyokouki.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddInitialArchiveSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateTable(
                name: "days",
                columns: table => new
                {
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    is_ready = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_days", x => x.date);
                });

            migrationBuilder.CreateTable(
                name: "day_translations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    day_date = table.Column<DateOnly>(type: "date", nullable: false),
                    language_code = table.Column<string>(type: "text", nullable: false),
                    note = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_day_translations", x => x.id);
                    table.ForeignKey(
                        name: "fk_day_translations_days_day_date",
                        column: x => x.day_date,
                        principalTable: "days",
                        principalColumn: "date",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "media_files",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    day_date = table.Column<DateOnly>(type: "date", nullable: false),
                    file_name = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    is_approved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_media_files", x => x.id);
                    table.ForeignKey(
                        name: "fk_media_files_days_day_date",
                        column: x => x.day_date,
                        principalTable: "days",
                        principalColumn: "date",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "media_translations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    media_file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_code = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_media_translations", x => x.id);
                    table.ForeignKey(
                        name: "fk_media_translations_media_files_media_file_id",
                        column: x => x.media_file_id,
                        principalTable: "media_files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_day_translations_day_date_language_code",
                table: "day_translations",
                columns: new[] { "day_date", "language_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_day_translations_note",
                table: "day_translations",
                column: "note")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_media_files_day_date",
                table: "media_files",
                column: "day_date");

            migrationBuilder.CreateIndex(
                name: "ix_media_translations_description",
                table: "media_translations",
                column: "description")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_media_translations_media_file_id_language_code",
                table: "media_translations",
                columns: new[] { "media_file_id", "language_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_media_translations_tags",
                table: "media_translations",
                column: "tags")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_media_translations_title",
                table: "media_translations",
                column: "title")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "day_translations");

            migrationBuilder.DropTable(
                name: "media_translations");

            migrationBuilder.DropTable(
                name: "media_files");

            migrationBuilder.DropTable(
                name: "days");
        }
    }
}
