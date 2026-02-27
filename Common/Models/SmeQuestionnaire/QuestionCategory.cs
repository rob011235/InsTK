// <copyright file="QuestionCategory.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Common.Models.SmeQuestionnaire
{
    /// <summary>
    /// Defines questionnaire section categories.
    /// </summary>
    public enum QuestionCategory
    {
        /// <summary>
        /// General context and background questions.
        /// </summary>
        General,

        /// <summary>
        /// Questions about skills and competencies.
        /// </summary>
        Skills,

        /// <summary>
        /// Questions about equipment used in practice.
        /// </summary>
        Equipment,

        /// <summary>
        /// Questions about software used in practice.
        /// </summary>
        Software,
    }
}
