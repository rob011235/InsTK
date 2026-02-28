// <copyright file="ISmeQuestionnaireDataService.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Common.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Common.Models.SmeQuestionnaire;

    /// <summary>
    /// Provides data access operations for SME questionnaire workflow pages.
    /// </summary>
    public interface ISmeQuestionnaireDataService
    {
        /// <summary>
        /// Gets all questionnaires.
        /// </summary>
        /// <returns>A list of questionnaires ordered by creation date.</returns>
        public Task<List<SmeQuestionnaire>> GetAllQuestionnairesAsync();

        /// <summary>
        /// Gets all curriculums.
        /// </summary>
        /// <returns>A list of curriculums ordered by title.</returns>
        public Task<List<Curriculum>> GetCurriculumsAsync();

        /// <summary>
        /// Creates a new curriculum.
        /// </summary>
        /// <param name="curriculum">The curriculum to create.</param>
        /// <returns>The created curriculum.</returns>
        public Task<Curriculum> CreateCurriculumAsync(Curriculum curriculum);

        /// <summary>
        /// Creates a new questionnaire definition.
        /// </summary>
        /// <param name="questionnaire">The questionnaire to create.</param>
        /// <returns>The created questionnaire.</returns>
        public Task<SmeQuestionnaire> CreateQuestionnaireAsync(SmeQuestionnaire questionnaire);

        /// <summary>
        /// Gets active SME questionnaires that can be sent as invites.
        /// </summary>
        /// <returns>A list of active questionnaires.</returns>
        public Task<List<SmeQuestionnaire>> GetActiveQuestionnairesAsync();

        /// <summary>
        /// Gets available subject matter experts.
        /// </summary>
        /// <returns>A list of SMEs.</returns>
        public Task<List<SubjectMatterExpert>> GetSubjectMatterExpertsAsync();

        /// <summary>
        /// Gets all invite/response records for admin review.
        /// </summary>
        /// <returns>A list of invite rows with questionnaire and SME display information.</returns>
        public Task<List<SmeInviteListItem>> GetInviteListAsync();

        /// <summary>
        /// Creates a new subject matter expert.
        /// </summary>
        /// <param name="subjectMatterExpert">The SME to create.</param>
        /// <returns>The created SME.</returns>
        public Task<SubjectMatterExpert> CreateSubjectMatterExpertAsync(SubjectMatterExpert subjectMatterExpert);

        /// <summary>
        /// Creates a new draft questionnaire response that can be used as an invite token.
        /// </summary>
        /// <param name="questionnaireId">The questionnaire identifier.</param>
        /// <param name="subjectMatterExpertId">The SME identifier.</param>
        /// <returns>The created response.</returns>
        public Task<SmeQuestionnaireResponse> CreateInviteResponseAsync(Guid questionnaireId, Guid subjectMatterExpertId);

        /// <summary>
        /// Gets an SME questionnaire response by its identifier.
        /// </summary>
        /// <param name="responseId">The response identifier.</param>
        /// <returns>The matching response if found; otherwise <see langword="null"/>.</returns>
        public Task<SmeQuestionnaireResponse?> GetResponseByIdAsync(Guid responseId);

        /// <summary>
        /// Gets the subject matter expert profile associated with a questionnaire response.
        /// </summary>
        /// <param name="responseId">The response identifier.</param>
        /// <returns>The SME profile if found; otherwise <see langword="null"/>.</returns>
        public Task<SubjectMatterExpert?> GetSmeProfileByResponseIdAsync(Guid responseId);

        /// <summary>
        /// Updates the subject matter expert profile associated with a questionnaire response.
        /// </summary>
        /// <param name="responseId">The response identifier.</param>
        /// <param name="updatedProfile">The profile values to persist.</param>
        /// <returns><see langword="true"/> if updated; otherwise <see langword="false"/>.</returns>
        public Task<bool> UpdateSmeProfileByResponseIdAsync(Guid responseId, SubjectMatterExpert updatedProfile);

        /// <summary>
        /// Gets skill rows associated with a questionnaire response.
        /// </summary>
        /// <param name="responseId">The response identifier.</param>
        /// <returns>A list of skills for the response.</returns>
        public Task<List<SmeSkill>> GetSkillsByResponseIdAsync(Guid responseId);

        /// <summary>
        /// Replaces the skill rows associated with a questionnaire response.
        /// </summary>
        /// <param name="responseId">The response identifier.</param>
        /// <param name="skills">The skills to persist.</param>
        /// <returns><see langword="true"/> if the response exists and was updated; otherwise <see langword="false"/>.</returns>
        public Task<bool> SaveSkillsByResponseIdAsync(Guid responseId, List<SmeSkill> skills);

        /// <summary>
        /// Gets equipment rows associated with a questionnaire response.
        /// </summary>
        /// <param name="responseId">The response identifier.</param>
        /// <returns>A list of equipment for the response.</returns>
        public Task<List<SmeEquipment>> GetEquipmentByResponseIdAsync(Guid responseId);

        /// <summary>
        /// Replaces the equipment rows associated with a questionnaire response.
        /// </summary>
        /// <param name="responseId">The response identifier.</param>
        /// <param name="equipment">The equipment to persist.</param>
        /// <returns><see langword="true"/> if the response exists and was updated; otherwise <see langword="false"/>.</returns>
        public Task<bool> SaveEquipmentByResponseIdAsync(Guid responseId, List<SmeEquipment> equipment);

        /// <summary>
        /// Sets the privacy acknowledgment timestamp for the response if one does not already exist.
        /// </summary>
        /// <param name="responseId">The response identifier.</param>
        /// <returns>
        /// The persisted acknowledgment timestamp if the response exists and has or receives an acknowledgment;
        /// otherwise <see langword="null"/>.
        /// </returns>
        public Task<DateTimeOffset?> AcknowledgePrivacyAsync(Guid responseId);
    }
}
