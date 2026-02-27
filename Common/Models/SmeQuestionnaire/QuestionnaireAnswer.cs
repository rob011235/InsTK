// <copyright file="QuestionnaireAnswer.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Common.Models.SmeQuestionnaire
{
    using System;

    /// <summary>
    /// Represents a typed answer to a questionnaire question.
    /// </summary>
    public class QuestionnaireAnswer
    {
        /// <summary>
        /// Gets or sets the answer identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the parent response identifier.
        /// </summary>
        public Guid ResponseId { get; set; }

        /// <summary>
        /// Gets or sets the question identifier.
        /// </summary>
        public Guid QuestionId { get; set; }

        /// <summary>
        /// Gets or sets text answer value.
        /// </summary>
        public string? ValueText { get; set; }

        /// <summary>
        /// Gets or sets numeric answer value.
        /// </summary>
        public decimal? ValueNumber { get; set; }

        /// <summary>
        /// Gets or sets boolean answer value.
        /// </summary>
        public bool? ValueBool { get; set; }

        /// <summary>
        /// Gets or sets rating answer value.
        /// </summary>
        public int? ValueRating { get; set; }
    }
}
