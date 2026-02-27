// <copyright file="20260227194920_AddSmeQuestionnaireModels.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

#nullable disable

namespace Server.Data.Migrations
{
    using System;
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <inheritdoc />
    public partial class AddSmeQuestionnaireModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Curriculums",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    TargetStartDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    TargetEndDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Curriculums", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SmeQuestionnaires",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CurriculumId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    FinalOpenEndedPrompt = table.Column<string>(type: "TEXT", nullable: true),
                    IsFinalOpenEndedRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmeQuestionnaires", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmeQuestionnaires_Curriculums_CurriculumId",
                        column: x => x.CurriculumId,
                        principalTable: "Curriculums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubjectMatterExperts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", nullable: false),
                    Company = table.Column<string>(type: "TEXT", nullable: true),
                    JobTitle = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    Website = table.Column<string>(type: "TEXT", nullable: true),
                    Location = table.Column<string>(type: "TEXT", nullable: true),
                    CurriculumId = table.Column<Guid>(type: "TEXT", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectMatterExperts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubjectMatterExperts_Curriculums_CurriculumId",
                        column: x => x.CurriculumId,
                        principalTable: "Curriculums",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "QuestionnaireSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuestionnaireId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Instructions = table.Column<string>(type: "TEXT", nullable: true),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    SmeQuestionnaireId = table.Column<Guid>(type: "TEXT", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionnaireSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionnaireSections_SmeQuestionnaires_SmeQuestionnaireId",
                        column: x => x.SmeQuestionnaireId,
                        principalTable: "SmeQuestionnaires",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SmeQuestionnaireResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuestionnaireId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SubjectMatterExpertId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedOn = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    SubmittedOn = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    PrivacyAcknowledgedOn = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    ReviewerNotes = table.Column<string>(type: "TEXT", nullable: true),
                    FinalOpenEndedAnswer = table.Column<string>(type: "TEXT", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmeQuestionnaireResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmeQuestionnaireResponses_SubjectMatterExperts_SubjectMatterExpertId",
                        column: x => x.SubjectMatterExpertId,
                        principalTable: "SubjectMatterExperts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionnaireQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SectionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Prompt = table.Column<string>(type: "TEXT", nullable: false),
                    InputType = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    HelpText = table.Column<string>(type: "TEXT", nullable: true),
                    QuestionnaireSectionId = table.Column<Guid>(type: "TEXT", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionnaireQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionnaireQuestions_QuestionnaireSections_QuestionnaireSectionId",
                        column: x => x.QuestionnaireSectionId,
                        principalTable: "QuestionnaireSections",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "QuestionnaireAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ResponseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuestionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ValueText = table.Column<string>(type: "TEXT", nullable: true),
                    ValueNumber = table.Column<decimal>(type: "TEXT", nullable: true),
                    ValueBool = table.Column<bool>(type: "INTEGER", nullable: true),
                    ValueRating = table.Column<int>(type: "INTEGER", nullable: true),
                    SmeQuestionnaireResponseId = table.Column<Guid>(type: "TEXT", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionnaireAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionnaireAnswers_SmeQuestionnaireResponses_SmeQuestionnaireResponseId",
                        column: x => x.SmeQuestionnaireResponseId,
                        principalTable: "SmeQuestionnaireResponses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SmeEquipment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ResponseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Manufacturer = table.Column<string>(type: "TEXT", nullable: true),
                    Model = table.Column<string>(type: "TEXT", nullable: true),
                    VersionOrSpec = table.Column<string>(type: "TEXT", nullable: true),
                    Frequency = table.Column<int>(type: "INTEGER", nullable: false),
                    RequiredForJob = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    SmeQuestionnaireResponseId = table.Column<Guid>(type: "TEXT", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmeEquipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmeEquipment_SmeQuestionnaireResponses_SmeQuestionnaireResponseId",
                        column: x => x.SmeQuestionnaireResponseId,
                        principalTable: "SmeQuestionnaireResponses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SmeSkills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ResponseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Proficiency = table.Column<int>(type: "INTEGER", nullable: false),
                    YearsExperience = table.Column<int>(type: "INTEGER", nullable: true),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    SmeQuestionnaireResponseId = table.Column<Guid>(type: "TEXT", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmeSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmeSkills_SmeQuestionnaireResponses_SmeQuestionnaireResponseId",
                        column: x => x.SmeQuestionnaireResponseId,
                        principalTable: "SmeQuestionnaireResponses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SmeSoftware",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ResponseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Vendor = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<string>(type: "TEXT", nullable: true),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    Frequency = table.Column<int>(type: "INTEGER", nullable: false),
                    RequiredForJob = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    SmeQuestionnaireResponseId = table.Column<Guid>(type: "TEXT", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmeSoftware", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmeSoftware_SmeQuestionnaireResponses_SmeQuestionnaireResponseId",
                        column: x => x.SmeQuestionnaireResponseId,
                        principalTable: "SmeQuestionnaireResponses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "QuestionOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuestionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    QuestionnaireQuestionId = table.Column<Guid>(type: "TEXT", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionOptions_QuestionnaireQuestions_QuestionnaireQuestionId",
                        column: x => x.QuestionnaireQuestionId,
                        principalTable: "QuestionnaireQuestions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireAnswers_SmeQuestionnaireResponseId",
                table: "QuestionnaireAnswers",
                column: "SmeQuestionnaireResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireQuestions_QuestionnaireSectionId",
                table: "QuestionnaireQuestions",
                column: "QuestionnaireSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireSections_SmeQuestionnaireId",
                table: "QuestionnaireSections",
                column: "SmeQuestionnaireId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptions_QuestionnaireQuestionId",
                table: "QuestionOptions",
                column: "QuestionnaireQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_SmeEquipment_SmeQuestionnaireResponseId",
                table: "SmeEquipment",
                column: "SmeQuestionnaireResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_SmeQuestionnaireResponses_SubjectMatterExpertId",
                table: "SmeQuestionnaireResponses",
                column: "SubjectMatterExpertId");

            migrationBuilder.CreateIndex(
                name: "IX_SmeQuestionnaires_CurriculumId",
                table: "SmeQuestionnaires",
                column: "CurriculumId");

            migrationBuilder.CreateIndex(
                name: "IX_SmeSkills_SmeQuestionnaireResponseId",
                table: "SmeSkills",
                column: "SmeQuestionnaireResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_SmeSoftware_SmeQuestionnaireResponseId",
                table: "SmeSoftware",
                column: "SmeQuestionnaireResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectMatterExperts_CurriculumId",
                table: "SubjectMatterExperts",
                column: "CurriculumId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionnaireAnswers");

            migrationBuilder.DropTable(
                name: "QuestionOptions");

            migrationBuilder.DropTable(
                name: "SmeEquipment");

            migrationBuilder.DropTable(
                name: "SmeSkills");

            migrationBuilder.DropTable(
                name: "SmeSoftware");

            migrationBuilder.DropTable(
                name: "QuestionnaireQuestions");

            migrationBuilder.DropTable(
                name: "SmeQuestionnaireResponses");

            migrationBuilder.DropTable(
                name: "QuestionnaireSections");

            migrationBuilder.DropTable(
                name: "SubjectMatterExperts");

            migrationBuilder.DropTable(
                name: "SmeQuestionnaires");

            migrationBuilder.DropTable(
                name: "Curriculums");
        }
    }
}
