// <copyright file="SmeInviteListItem.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Common.Models.SmeQuestionnaire
{
    using System;

    /// <summary>
    /// Represents an admin-facing view of an SME invite/response record.
    /// </summary>
    public class SmeInviteListItem
    {
        /// <summary>
        /// Gets or sets the response identifier used in the invite link.
        /// </summary>
        public Guid ResponseId { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire title.
        /// </summary>
        public string QuestionnaireTitle { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the SME full name.
        /// </summary>
        public string SmeFullName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the SME email.
        /// </summary>
        public string? SmeEmail { get; set; }

        /// <summary>
        /// Gets or sets the response status.
        /// </summary>
        public ResponseStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the time the response invite was created/started.
        /// </summary>
        public DateTimeOffset StartedOn { get; set; }

        /// <summary>
        /// Gets or sets the time the privacy notice was acknowledged.
        /// </summary>
        public DateTimeOffset? PrivacyAcknowledgedOn { get; set; }

        /// <summary>
        /// Gets or sets the time the response was submitted.
        /// </summary>
        public DateTimeOffset? SubmittedOn { get; set; }
    }
}
