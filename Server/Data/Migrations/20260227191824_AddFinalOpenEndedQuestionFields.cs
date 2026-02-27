using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFinalOpenEndedQuestionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FinalOpenEndedPrompt",
                table: "SmeQuestionnaires",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFinalOpenEndedRequired",
                table: "SmeQuestionnaires",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FinalOpenEndedAnswer",
                table: "SmeQuestionnaireResponses",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinalOpenEndedPrompt",
                table: "SmeQuestionnaires");

            migrationBuilder.DropColumn(
                name: "IsFinalOpenEndedRequired",
                table: "SmeQuestionnaires");

            migrationBuilder.DropColumn(
                name: "FinalOpenEndedAnswer",
                table: "SmeQuestionnaireResponses");
        }
    }
}
