// <copyright file="QuestionnaireSection.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Common.Models.SmeQuestionnaire
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a section within an SME questionnaire.
    /// </summary>
    public class QuestionnaireSection
    {
        /// <summary>
        /// Gets or sets the section identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the parent questionnaire identifier.
        /// </summary>
        public Guid QuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the section category.
        /// </summary>
        public QuestionCategory Category { get; set; }

        /// <summary>
        /// Gets or sets the section title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets instructions shown for this section.
        /// </summary>
        public string? Instructions { get; set; }

        /// <summary>
        /// Gets or sets display ordering value.
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the questions in this section.
        /// </summary>
        public List<QuestionnaireQuestion> Questions { get; set; } = new List<QuestionnaireQuestion>();
    }
}
