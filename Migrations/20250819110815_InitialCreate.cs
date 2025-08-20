using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace bookmarkr.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bookmarks",
                columns: table => new
                {
                    BookmarkId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    URL = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookmarks", x => x.BookmarkId);
                });

            migrationBuilder.InsertData(
                table: "Bookmarks",
                columns: new[] { "BookmarkId", "Category", "CreatedAt", "Name", "UpdatedAt", "URL" },
                values: new object[,]
                {
                    { 1, "Cars", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "First", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://www.lol.com" },
                    { 2, "Tech", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Second", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://www.second.com" },
                    { 3, "Tech", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Third", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://www.third.com" },
                    { 4, "Cooking", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Fourth", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://www.cooking.com" },
                    { 5, "SocialMedia", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Fifth", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://www.social.com" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_Name_Unique",
                table: "Bookmarks",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_Url_Unique",
                table: "Bookmarks",
                column: "URL",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookmarks");
        }
    }
}
