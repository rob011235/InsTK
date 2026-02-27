// <copyright file="Curriculum.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Common.Models.SmeQuestionnaire
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a curriculum that can contain SME questionnaires.
    /// </summary>
    public class Curriculum
    {
        /// <summary>
        /// Gets or sets the curriculum identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the curriculum title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the curriculum description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the target start date.
        /// </summary>
        public DateTimeOffset? TargetStartDate { get; set; }

        /// <summary>
        /// Gets or sets the target end date.
        /// </summary>
        public DateTimeOffset? TargetEndDate { get; set; }

        /// <summary>
        /// Gets or sets the subject matter experts participating in this curriculum.
        /// </summary>
        public List<SubjectMatterExpert> SubjectMatterExperts { get; set; } = new List<SubjectMatterExpert>();

        /// <summary>
        /// Gets or sets the questionnaires associated with this curriculum.
        /// </summary>
        public List<SmeQuestionnaire> Questionnaires { get; set; } = new List<SmeQuestionnaire>();
    }
}
