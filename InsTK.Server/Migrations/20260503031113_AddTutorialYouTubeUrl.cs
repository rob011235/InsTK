using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsTK.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddTutorialYouTubeUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "YouTubeUrl",
                table: "Tutorials",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "YouTubeUrl",
                table: "Tutorials");
        }
    }
}
