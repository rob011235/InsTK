// <copyright file="ProficiencyLevel.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Common.Models.SmeQuestionnaire
{
    /// <summary>
    /// Defines skill proficiency levels.
    /// </summary>
    public enum ProficiencyLevel
    {
        /// <summary>
        /// Foundational familiarity with the skill.
        /// </summary>
        Beginner,

        /// <summary>
        /// Practical working proficiency with occasional guidance.
        /// </summary>
        Intermediate,

        /// <summary>
        /// Strong independent proficiency in the skill.
        /// </summary>
        Advanced,

        /// <summary>
        /// Deep subject-level expertise in the skill.
        /// </summary>
        Expert,
    }
}
