// <copyright file="QuestionnaireQuestion.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Common.Models.SmeQuestionnaire
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a single question definition in a questionnaire section.
    /// </summary>
    public class QuestionnaireQuestion
    {
        /// <summary>
        /// Gets or sets the question identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the parent section identifier.
        /// </summary>
        public Guid SectionId { get; set; }

        /// <summary>
        /// Gets or sets the prompt shown to the SME.
        /// </summary>
        public string Prompt { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the expected input type.
        /// </summary>
        public QuestionInputType InputType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this question is required.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets display ordering value.
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets optional help text.
        /// </summary>
        public string? HelpText { get; set; }

        /// <summary>
        /// Gets or sets answer options for select-style questions.
        /// </summary>
        public List<QuestionOption> Options { get; set; } = new List<QuestionOption>();
    }
}
