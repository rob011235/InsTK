using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsTK.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReviseTutorialStepDesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EvidenceHints");

            migrationBuilder.AddColumn<string>(
                name: "Technology",
                table: "Tutorials",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.RenameColumn(
                name: "Heading",
                table: "TutorialSteps",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "MarkdownContent",
                table: "TutorialSteps",
                newName: "InstructionMarkdown");

            migrationBuilder.AddColumn<string>(
                name: "GradingHints",
                table: "TutorialSteps",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceCode",
                table: "TutorialSteps",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceFileName",
                table: "TutorialSteps",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Technology",
                table: "Tutorials");

            migrationBuilder.DropColumn(
                name: "GradingHints",
                table: "TutorialSteps");

            migrationBuilder.DropColumn(
                name: "ReferenceCode",
                table: "TutorialSteps");

            migrationBuilder.DropColumn(
                name: "ReferenceFileName",
                table: "TutorialSteps");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "TutorialSteps",
                newName: "Heading");

            migrationBuilder.RenameColumn(
                name: "InstructionMarkdown",
                table: "TutorialSteps",
                newName: "MarkdownContent");

            migrationBuilder.CreateTable(
                name: "EvidenceHints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TutorialStepId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvidenceHints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvidenceHints_TutorialSteps_TutorialStepId",
                        column: x => x.TutorialStepId,
                        principalTable: "TutorialSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EvidenceHints_TutorialStepId",
                table: "EvidenceHints",
                column: "TutorialStepId");
        }
    }
}
