// <copyright file="ResponseStatus.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Common.Models.SmeQuestionnaire
{
    /// <summary>
    /// Defines status values for questionnaire responses.
    /// </summary>
    public enum ResponseStatus
    {
        /// <summary>
        /// Response has been started but not submitted.
        /// </summary>
        Draft,

        /// <summary>
        /// Response was submitted by the SME.
        /// </summary>
        Submitted,

        /// <summary>
        /// Response has been reviewed by staff.
        /// </summary>
        Reviewed,
    }
}
