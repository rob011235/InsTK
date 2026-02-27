// <copyright file="SubjectMatterExpert.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Common.Models.SmeQuestionnaire
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a subject matter expert who completes questionnaires.
    /// </summary>
    public class SubjectMatterExpert
    {
        /// <summary>
        /// Gets or sets the SME identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the full name.
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the company.
        /// </summary>
        public string? Company { get; set; }

        /// <summary>
        /// Gets or sets the job title.
        /// </summary>
        public string? JobTitle { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Gets or sets the website.
        /// </summary>
        public string? Website { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire responses submitted by the SME.
        /// </summary>
        public List<SmeQuestionnaireResponse> QuestionnaireResponses { get; set; } = new List<SmeQuestionnaireResponse>();
    }
}
