// <copyright file="SmeQuestionnaire.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Common.Models.SmeQuestionnaire
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a questionnaire template sent to SMEs.
    /// </summary>
    public class SmeQuestionnaire
    {
        /// <summary>
        /// Gets or sets the questionnaire identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the curriculum identifier.
        /// </summary>
        public Guid CurriculumId { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the questionnaire description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this questionnaire is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp.
        /// </summary>
        public DateTimeOffset CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets the sections contained in this questionnaire.
        /// </summary>
        public List<QuestionnaireSection> Sections { get; set; } = new List<QuestionnaireSection>();
    }
}
