// <copyright file="SmeQuestionnaireDataService.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Server.Data.Services
{
    using System;
    using System.Collections.Generic;
    using Common.Interfaces;
    using Common.Models.SmeQuestionnaire;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Provides data access methods for SME questionnaire pages.
    /// </summary>
    public class SmeQuestionnaireDataService : ISmeQuestionnaireDataService
    {
        private readonly ApplicationDbContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmeQuestionnaireDataService"/> class.
        /// </summary>
        /// <param name="context">The database context to use for data operations.</param>
        public SmeQuestionnaireDataService(ApplicationDbContext context)
        {
            this.context = context;
        }

        /// <inheritdoc/>
        public async Task<List<SmeQuestionnaire>> GetAllQuestionnairesAsync()
        {
            List<SmeQuestionnaire> questionnaires = await this.context.SmeQuestionnaires.ToListAsync();
            return questionnaires
                .OrderByDescending(x => x.CreatedOn)
                .ToList();
        }

        /// <inheritdoc/>
        public async Task<List<Curriculum>> GetCurriculumsAsync()
        {
            return await this.context.Curriculums
                .OrderBy(x => x.Title)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Curriculum> CreateCurriculumAsync(Curriculum curriculum)
        {
            curriculum.Id = Guid.NewGuid();
            this.context.Curriculums.Add(curriculum);
            await this.context.SaveChangesAsync();
            return curriculum;
        }

        /// <inheritdoc/>
        public async Task<SmeQuestionnaire> CreateQuestionnaireAsync(SmeQuestionnaire questionnaire)
        {
            questionnaire.Id = Guid.NewGuid();
            questionnaire.CreatedOn = DateTimeOffset.UtcNow;

            this.context.SmeQuestionnaires.Add(questionnaire);
            await this.context.SaveChangesAsync();
            return questionnaire;
        }

        /// <inheritdoc/>
        public async Task<List<SmeQuestionnaire>> GetActiveQuestionnairesAsync()
        {
            return await this.context.SmeQuestionnaires
                .Where(x => x.IsActive)
                .OrderBy(x => x.Title)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<SubjectMatterExpert>> GetSubjectMatterExpertsAsync()
        {
            return await this.context.SubjectMatterExperts
                .OrderBy(x => x.FullName)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<SmeInviteListItem>> GetInviteListAsync()
        {
            List<SmeQuestionnaireResponse> responses = await this.context.SmeQuestionnaireResponses.ToListAsync();
            List<SmeQuestionnaire> questionnaires = await this.context.SmeQuestionnaires.ToListAsync();
            List<SubjectMatterExpert> smes = await this.context.SubjectMatterExperts.ToListAsync();

            Dictionary<Guid, SmeQuestionnaire> questionnaireById = questionnaires
                .ToDictionary(x => x.Id, x => x);
            Dictionary<Guid, SubjectMatterExpert> smeById = smes
                .ToDictionary(x => x.Id, x => x);

            return responses
                .Select(response =>
                {
                    questionnaireById.TryGetValue(response.QuestionnaireId, out SmeQuestionnaire? questionnaire);
                    smeById.TryGetValue(response.SubjectMatterExpertId, out SubjectMatterExpert? sme);

                    return new SmeInviteListItem
                    {
                        ResponseId = response.Id,
                        QuestionnaireTitle = questionnaire?.Title ?? "(Unknown Questionnaire)",
                        SmeFullName = sme?.FullName ?? "(Unknown SME)",
                        SmeEmail = sme?.Email,
                        Status = response.Status,
                        StartedOn = response.StartedOn,
                        PrivacyAcknowledgedOn = response.PrivacyAcknowledgedOn,
                        SubmittedOn = response.SubmittedOn,
                    };
                })
                .OrderByDescending(x => x.StartedOn)
                .ToList();
        }

        /// <inheritdoc/>
        public async Task<SubjectMatterExpert> CreateSubjectMatterExpertAsync(SubjectMatterExpert subjectMatterExpert)
        {
            subjectMatterExpert.Id = Guid.NewGuid();
            this.context.SubjectMatterExperts.Add(subjectMatterExpert);
            await this.context.SaveChangesAsync();
            return subjectMatterExpert;
        }

        /// <inheritdoc/>
        public async Task<SmeQuestionnaireResponse> CreateInviteResponseAsync(Guid questionnaireId, Guid subjectMatterExpertId)
        {
            bool questionnaireExists = await this.context.SmeQuestionnaires
                .AnyAsync(x => x.Id == questionnaireId && x.IsActive);

            if (!questionnaireExists)
            {
                throw new InvalidOperationException("Questionnaire not found or inactive.");
            }

            bool smeExists = await this.context.SubjectMatterExperts
                .AnyAsync(x => x.Id == subjectMatterExpertId);

            if (!smeExists)
            {
                throw new InvalidOperationException("Subject matter expert not found.");
            }

            SmeQuestionnaireResponse response = new SmeQuestionnaireResponse
            {
                Id = Guid.NewGuid(),
                QuestionnaireId = questionnaireId,
                SubjectMatterExpertId = subjectMatterExpertId,
                Status = ResponseStatus.Draft,
                StartedOn = DateTimeOffset.UtcNow,
            };

            this.context.SmeQuestionnaireResponses.Add(response);
            await this.context.SaveChangesAsync();
            return response;
        }

        /// <inheritdoc/>
        public async Task<SmeQuestionnaireResponse?> GetResponseByIdAsync(Guid responseId)
        {
            return await this.context.SmeQuestionnaireResponses
                .FirstOrDefaultAsync(x => x.Id == responseId);
        }

        /// <inheritdoc/>
        public async Task<DateTimeOffset?> AcknowledgePrivacyAsync(Guid responseId)
        {
            SmeQuestionnaireResponse? response = await this.context.SmeQuestionnaireResponses
                .FirstOrDefaultAsync(x => x.Id == responseId);

            if (response is null)
            {
                return null;
            }

            if (!response.PrivacyAcknowledgedOn.HasValue)
            {
                response.PrivacyAcknowledgedOn = DateTimeOffset.UtcNow;
                await this.context.SaveChangesAsync();
            }

            return response.PrivacyAcknowledgedOn;
        }
    }
}
