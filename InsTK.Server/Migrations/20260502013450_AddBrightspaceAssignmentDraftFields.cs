using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsTK.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddBrightspaceAssignmentDraftFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BrightspaceAssignmentInstructions",
                table: "Tutorials",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BrightspaceAssignmentTitle",
                table: "Tutorials",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BrightspacePoints",
                table: "Tutorials",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BrightspaceSubmissionInstructions",
                table: "Tutorials",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrightspaceAssignmentInstructions",
                table: "Tutorials");

            migrationBuilder.DropColumn(
                name: "BrightspaceAssignmentTitle",
                table: "Tutorials");

            migrationBuilder.DropColumn(
                name: "BrightspacePoints",
                table: "Tutorials");

            migrationBuilder.DropColumn(
                name: "BrightspaceSubmissionInstructions",
                table: "Tutorials");
        }
    }
}
