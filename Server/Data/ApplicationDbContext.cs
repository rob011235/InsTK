// <copyright file="ApplicationDbContext.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Server.Data
{
    using Common.Models;
    using Common.Models.SmeQuestionnaire;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Represents the application's database context, including Identity and custom entities.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the collection of courses in the database.
        /// </summary>
        public DbSet<Course> Courses { get; set; }

        /// <summary>
        /// Gets or sets the collection of objectives in the database.
        /// </summary>
        public DbSet<Objective> Objectives { get; set; }

        /// <summary>
        /// Gets or sets the collection of curriculums in the database.
        /// </summary>
        public DbSet<Curriculum> Curriculums { get; set; }

        /// <summary>
        /// Gets or sets the collection of subject matter experts in the database.
        /// </summary>
        public DbSet<SubjectMatterExpert> SubjectMatterExperts { get; set; }

        /// <summary>
        /// Gets or sets the collection of SME questionnaires in the database.
        /// </summary>
        public DbSet<SmeQuestionnaire> SmeQuestionnaires { get; set; }

        /// <summary>
        /// Gets or sets the collection of questionnaire sections in the database.
        /// </summary>
        public DbSet<QuestionnaireSection> QuestionnaireSections { get; set; }

        /// <summary>
        /// Gets or sets the collection of questionnaire questions in the database.
        /// </summary>
        public DbSet<QuestionnaireQuestion> QuestionnaireQuestions { get; set; }

        /// <summary>
        /// Gets or sets the collection of question options in the database.
        /// </summary>
        public DbSet<QuestionOption> QuestionOptions { get; set; }

        /// <summary>
        /// Gets or sets the collection of questionnaire responses in the database.
        /// </summary>
        public DbSet<SmeQuestionnaireResponse> SmeQuestionnaireResponses { get; set; }

        /// <summary>
        /// Gets or sets the collection of questionnaire answers in the database.
        /// </summary>
        public DbSet<QuestionnaireAnswer> QuestionnaireAnswers { get; set; }

        /// <summary>
        /// Gets or sets the collection of SME skills in the database.
        /// </summary>
        public DbSet<SmeSkill> SmeSkills { get; set; }

        /// <summary>
        /// Gets or sets the collection of SME equipment in the database.
        /// </summary>
        public DbSet<SmeEquipment> SmeEquipment { get; set; }

        /// <summary>
        /// Gets or sets the collection of SME software in the database.
        /// </summary>
        public DbSet<SmeSoftware> SmeSoftware { get; set; }
    }
}
