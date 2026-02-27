// <copyright file="SmeQuestionnaireResponse.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Common.Models.SmeQuestionnaire
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a single SME submission for a questionnaire.
    /// </summary>
    public class SmeQuestionnaireResponse
    {
        /// <summary>
        /// Gets or sets the response identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire identifier.
        /// </summary>
        public Guid QuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the SME identifier.
        /// </summary>
        public Guid SubjectMatterExpertId { get; set; }

        /// <summary>
        /// Gets or sets the response status.
        /// </summary>
        public ResponseStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the time the response was started.
        /// </summary>
        public DateTimeOffset StartedOn { get; set; }

        /// <summary>
        /// Gets or sets the time the response was submitted.
        /// </summary>
        public DateTimeOffset? SubmittedOn { get; set; }

        /// <summary>
        /// Gets or sets reviewer notes.
        /// </summary>
        public string? ReviewerNotes { get; set; }

        /// <summary>
        /// Gets or sets raw answers to questionnaire questions.
        /// </summary>
        public List<QuestionnaireAnswer> Answers { get; set; } = new List<QuestionnaireAnswer>();

        /// <summary>
        /// Gets or sets normalized skills captured from the response.
        /// </summary>
        public List<SmeSkill> Skills { get; set; } = new List<SmeSkill>();

        /// <summary>
        /// Gets or sets normalized equipment captured from the response.
        /// </summary>
        public List<SmeEquipment> Equipment { get; set; } = new List<SmeEquipment>();

        /// <summary>
        /// Gets or sets normalized software captured from the response.
        /// </summary>
        public List<SmeSoftware> Software { get; set; } = new List<SmeSoftware>();
    }
}
