// <copyright file="QuestionOption.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Common.Models.SmeQuestionnaire
{
    using System;

    /// <summary>
    /// Represents a selectable option for a questionnaire question.
    /// </summary>
    public class QuestionOption
    {
        /// <summary>
        /// Gets or sets the option identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the parent question identifier.
        /// </summary>
        public Guid QuestionId { get; set; }

        /// <summary>
        /// Gets or sets the option value.
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the option label.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets display ordering value.
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}
