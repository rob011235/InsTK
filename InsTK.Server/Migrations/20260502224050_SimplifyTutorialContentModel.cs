using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsTK.Server.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyTutorialContentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TutorialSteps");

            migrationBuilder.DropColumn(
                name: "ConclusionMarkdown",
                table: "Tutorials");

            migrationBuilder.RenameColumn(
                name: "IntroMarkdown",
                table: "Tutorials",
                newName: "ContentMarkdown");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ContentMarkdown",
                table: "Tutorials",
                newName: "IntroMarkdown");

            migrationBuilder.AddColumn<string>(
                name: "ConclusionMarkdown",
                table: "Tutorials",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TutorialSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TutorialDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    InstructionMarkdown = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorialSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TutorialSteps_Tutorials_TutorialDefinitionId",
                        column: x => x.TutorialDefinitionId,
                        principalTable: "Tutorials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TutorialSteps_TutorialDefinitionId",
                table: "TutorialSteps",
                column: "TutorialDefinitionId");
        }
    }
}
