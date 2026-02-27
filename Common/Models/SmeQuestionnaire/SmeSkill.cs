// <copyright file="SmeSkill.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Common.Models.SmeQuestionnaire
{
    using System;

    /// <summary>
    /// Represents a normalized skill captured from an SME response.
    /// </summary>
    public class SmeSkill
    {
        /// <summary>
        /// Gets or sets the skill identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the parent response identifier.
        /// </summary>
        public Guid ResponseId { get; set; }

        /// <summary>
        /// Gets or sets the skill name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the proficiency level.
        /// </summary>
        public ProficiencyLevel Proficiency { get; set; }

        /// <summary>
        /// Gets or sets years of experience.
        /// </summary>
        public int? YearsExperience { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a primary skill.
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// Gets or sets optional notes.
        /// </summary>
        public string? Notes { get; set; }
    }
}
