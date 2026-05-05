using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using InsTK.Server.Data;

#nullable disable

namespace InsTK.Server.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260505110000_RenameTutorialContentToHtml")]
    public partial class RenameTutorialContentToHtml : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ContentMarkdown",
                table: "Tutorials",
                newName: "ContentHtml");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ContentHtml",
                table: "Tutorials",
                newName: "ContentMarkdown");
        }
    }
}
