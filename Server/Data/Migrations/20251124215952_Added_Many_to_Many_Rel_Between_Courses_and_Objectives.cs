using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class Added_Many_to_Many_Rel_Between_Courses_and_Objectives : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseObjective",
                columns: table => new
                {
                    CoursesId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ObjectivesId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseObjective", x => new { x.CoursesId, x.ObjectivesId });
                    table.ForeignKey(
                        name: "FK_CourseObjective_Courses_CoursesId",
                        column: x => x.CoursesId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseObjective_Objectives_ObjectivesId",
                        column: x => x.ObjectivesId,
                        principalTable: "Objectives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseObjective_ObjectivesId",
                table: "CourseObjective",
                column: "ObjectivesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseObjective");
        }
    }
}
