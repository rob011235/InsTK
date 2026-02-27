// <copyright file="QuestionInputType.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Common.Models.SmeQuestionnaire
{
    /// <summary>
    /// Defines question input types.
    /// </summary>
    public enum QuestionInputType
    {
        /// <summary>
        /// Free-form text input.
        /// </summary>
        Text,

        /// <summary>
        /// Selection of multiple options.
        /// </summary>
        MultiSelect,

        /// <summary>
        /// Numeric input.
        /// </summary>
        Number,

        /// <summary>
        /// Boolean yes/no selection.
        /// </summary>
        YesNo,

        /// <summary>
        /// Ordinal rating input.
        /// </summary>
        Rating,
    }
}
