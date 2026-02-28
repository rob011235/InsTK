// <copyright file="SmeQuestionnaireDataService.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Server.Data.Services
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using Common.Interfaces;
    using Common.Models.SmeQuestionnaire;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Provides data access methods for SME questionnaire pages.
    /// </summary>
    public class SmeQuestionnaireDataService : ISmeQuestionnaireDataService
    {
        private static readonly Guid FacilityTourWillingQuestionId = new Guid("ef4f6c8f-9d52-4f49-9f40-bc309bfd4df8");
        private static readonly Guid FacilityTourDetailsQuestionId = new Guid("184ab5f0-b98a-4ecd-91e8-e92a093f2f4f");
        private static readonly Guid FacilityTourAvailabilitiesQuestionId = new Guid("90f4f4c5-9478-4d91-b0db-183fd2bb1a12");
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
        public async Task<SubjectMatterExpert?> GetSmeProfileByResponseIdAsync(Guid responseId)
        {
            SmeQuestionnaireResponse? response = await this.context.SmeQuestionnaireResponses
                .FirstOrDefaultAsync(x => x.Id == responseId);

            if (response is null)
            {
                return null;
            }

            return await this.context.SubjectMatterExperts
                .FirstOrDefaultAsync(x => x.Id == response.SubjectMatterExpertId);
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateSmeProfileByResponseIdAsync(Guid responseId, SubjectMatterExpert updatedProfile)
        {
            SmeQuestionnaireResponse? response = await this.context.SmeQuestionnaireResponses
                .FirstOrDefaultAsync(x => x.Id == responseId);

            if (response is null)
            {
                return false;
            }

            SubjectMatterExpert? existing = await this.context.SubjectMatterExperts
                .FirstOrDefaultAsync(x => x.Id == response.SubjectMatterExpertId);

            if (existing is null)
            {
                return false;
            }

            existing.FullName = updatedProfile.FullName;
            existing.Company = updatedProfile.Company;
            existing.JobTitle = updatedProfile.JobTitle;
            existing.Email = updatedProfile.Email;
            existing.Phone = updatedProfile.Phone;
            existing.Website = updatedProfile.Website;
            existing.Location = updatedProfile.Location;

            await this.context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<List<SmeSkill>> GetSkillsByResponseIdAsync(Guid responseId)
        {
            return await this.context.SmeSkills
                .Where(x => x.ResponseId == responseId)
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.Name)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> SaveSkillsByResponseIdAsync(Guid responseId, List<SmeSkill> skills)
        {
            bool responseExists = await this.context.SmeQuestionnaireResponses
                .AnyAsync(x => x.Id == responseId);

            if (!responseExists)
            {
                return false;
            }

            List<SmeSkill> existing = await this.context.SmeSkills
                .Where(x => x.ResponseId == responseId)
                .ToListAsync();

            if (existing.Count > 0)
            {
                this.context.SmeSkills.RemoveRange(existing);
            }

            List<SmeSkill> sanitized = skills
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .Select(x => new SmeSkill
                {
                    Id = Guid.NewGuid(),
                    ResponseId = responseId,
                    Name = x.Name.Trim(),
                    Proficiency = x.Proficiency,
                    YearsExperience = x.YearsExperience,
                    IsPrimary = x.IsPrimary,
                    Notes = string.IsNullOrWhiteSpace(x.Notes) ? null : x.Notes.Trim(),
                })
                .ToList();

            if (sanitized.Count > 0)
            {
                this.context.SmeSkills.AddRange(sanitized);
            }

            await this.context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<List<SmeEquipment>> GetEquipmentByResponseIdAsync(Guid responseId)
        {
            return await this.context.SmeEquipment
                .Where(x => x.ResponseId == responseId)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> SaveEquipmentByResponseIdAsync(Guid responseId, List<SmeEquipment> equipment)
        {
            bool responseExists = await this.context.SmeQuestionnaireResponses
                .AnyAsync(x => x.Id == responseId);

            if (!responseExists)
            {
                return false;
            }

            List<SmeEquipment> existing = await this.context.SmeEquipment
                .Where(x => x.ResponseId == responseId)
                .ToListAsync();

            if (existing.Count > 0)
            {
                this.context.SmeEquipment.RemoveRange(existing);
            }

            List<SmeEquipment> sanitized = equipment
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .Select(x => new SmeEquipment
                {
                    Id = Guid.NewGuid(),
                    ResponseId = responseId,
                    Name = x.Name.Trim(),
                    Type = x.Type,
                    Manufacturer = string.IsNullOrWhiteSpace(x.Manufacturer) ? null : x.Manufacturer.Trim(),
                    Model = string.IsNullOrWhiteSpace(x.Model) ? null : x.Model.Trim(),
                    VersionOrSpec = string.IsNullOrWhiteSpace(x.VersionOrSpec) ? null : x.VersionOrSpec.Trim(),
                    Frequency = x.Frequency,
                    RequiredForJob = x.RequiredForJob,
                    Notes = string.IsNullOrWhiteSpace(x.Notes) ? null : x.Notes.Trim(),
                })
                .ToList();

            if (sanitized.Count > 0)
            {
                this.context.SmeEquipment.AddRange(sanitized);
            }

            await this.context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<SmeFacilityTourPreference?> GetFacilityTourPreferenceByResponseIdAsync(Guid responseId)
        {
            bool responseExists = await this.context.SmeQuestionnaireResponses
                .AnyAsync(x => x.Id == responseId);

            if (!responseExists)
            {
                return null;
            }

            List<QuestionnaireAnswer> answers = await this.context.QuestionnaireAnswers
                .Where(x => x.ResponseId == responseId &&
                    (x.QuestionId == FacilityTourWillingQuestionId ||
                     x.QuestionId == FacilityTourDetailsQuestionId ||
                     x.QuestionId == FacilityTourAvailabilitiesQuestionId))
                .ToListAsync();

            QuestionnaireAnswer? willingAnswer = answers.FirstOrDefault(x => x.QuestionId == FacilityTourWillingQuestionId);
            QuestionnaireAnswer? detailsAnswer = answers.FirstOrDefault(x => x.QuestionId == FacilityTourDetailsQuestionId);
            QuestionnaireAnswer? availabilityAnswer = answers.FirstOrDefault(x => x.QuestionId == FacilityTourAvailabilitiesQuestionId);

            List<SmeFacilityTourAvailability> availabilities = new List<SmeFacilityTourAvailability>();
            if (!string.IsNullOrWhiteSpace(availabilityAnswer?.ValueText))
            {
                try
                {
                    availabilities = JsonSerializer.Deserialize<List<SmeFacilityTourAvailability>>(availabilityAnswer.ValueText) ??
                        new List<SmeFacilityTourAvailability>();
                }
                catch (JsonException)
                {
                    availabilities = new List<SmeFacilityTourAvailability>();
                }
            }

            return new SmeFacilityTourPreference
            {
                IsWillingToOfferTour = willingAnswer?.ValueBool,
                Details = string.IsNullOrWhiteSpace(detailsAnswer?.ValueText) ? null : detailsAnswer!.ValueText!.Trim(),
                Availabilities = availabilities,
            };
        }

        /// <inheritdoc/>
        public async Task<bool> SaveFacilityTourPreferenceByResponseIdAsync(Guid responseId, SmeFacilityTourPreference preference)
        {
            bool responseExists = await this.context.SmeQuestionnaireResponses
                .AnyAsync(x => x.Id == responseId);

            if (!responseExists)
            {
                return false;
            }

            List<QuestionnaireAnswer> existing = await this.context.QuestionnaireAnswers
                .Where(x => x.ResponseId == responseId &&
                    (x.QuestionId == FacilityTourWillingQuestionId ||
                     x.QuestionId == FacilityTourDetailsQuestionId ||
                     x.QuestionId == FacilityTourAvailabilitiesQuestionId))
                .ToListAsync();

            if (existing.Count > 0)
            {
                this.context.QuestionnaireAnswers.RemoveRange(existing);
            }

            List<QuestionnaireAnswer> answersToSave = new List<QuestionnaireAnswer>();

            if (preference.IsWillingToOfferTour.HasValue)
            {
                answersToSave.Add(new QuestionnaireAnswer
                {
                    Id = Guid.NewGuid(),
                    ResponseId = responseId,
                    QuestionId = FacilityTourWillingQuestionId,
                    ValueBool = preference.IsWillingToOfferTour.Value,
                });
            }

            if (!string.IsNullOrWhiteSpace(preference.Details))
            {
                answersToSave.Add(new QuestionnaireAnswer
                {
                    Id = Guid.NewGuid(),
                    ResponseId = responseId,
                    QuestionId = FacilityTourDetailsQuestionId,
                    ValueText = preference.Details.Trim(),
                });
            }

            List<SmeFacilityTourAvailability> sanitizedAvailabilities = preference.Availabilities
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.Date) ||
                    !string.IsNullOrWhiteSpace(x.StartTime) ||
                    !string.IsNullOrWhiteSpace(x.EndTime))
                .Select(x => new SmeFacilityTourAvailability
                {
                    Date = string.IsNullOrWhiteSpace(x.Date) ? null : x.Date.Trim(),
                    StartTime = string.IsNullOrWhiteSpace(x.StartTime) ? null : x.StartTime.Trim(),
                    EndTime = string.IsNullOrWhiteSpace(x.EndTime) ? null : x.EndTime.Trim(),
                })
                .ToList();

            if (sanitizedAvailabilities.Count > 0)
            {
                answersToSave.Add(new QuestionnaireAnswer
                {
                    Id = Guid.NewGuid(),
                    ResponseId = responseId,
                    QuestionId = FacilityTourAvailabilitiesQuestionId,
                    ValueText = JsonSerializer.Serialize(sanitizedAvailabilities),
                });
            }

            if (answersToSave.Count > 0)
            {
                this.context.QuestionnaireAnswers.AddRange(answersToSave);
            }

            await this.context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> MarkResponseSubmittedAsync(Guid responseId)
        {
            SmeQuestionnaireResponse? response = await this.context.SmeQuestionnaireResponses
                .FirstOrDefaultAsync(x => x.Id == responseId);

            if (response is null)
            {
                return false;
            }

            response.Status = ResponseStatus.Submitted;
            if (!response.SubmittedOn.HasValue)
            {
                response.SubmittedOn = DateTimeOffset.UtcNow;
            }

            await this.context.SaveChangesAsync();
            return true;
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
