// <copyright file="SmeSurveyPdfController.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Server.Controllers
{
    using Common.Interfaces;
    using Common.Models.SmeQuestionnaire;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using QuestPDF.Fluent;
    using QuestPDF.Helpers;
    using QuestPDF.Infrastructure;

    /// <summary>
    /// Provides PDF export endpoints for SME surveys.
    /// </summary>
    [ApiController]
    [Route("api/sme-surveys")]
    public class SmeSurveyPdfController : ControllerBase
    {
        private readonly ISmeQuestionnaireDataService smeQuestionnaireDataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmeSurveyPdfController"/> class.
        /// </summary>
        /// <param name="smeQuestionnaireDataService">The questionnaire data service.</param>
        public SmeSurveyPdfController(ISmeQuestionnaireDataService smeQuestionnaireDataService)
        {
            this.smeQuestionnaireDataService = smeQuestionnaireDataService;
        }

        /// <summary>
        /// Downloads a PDF representation of an SME survey response.
        /// </summary>
        /// <param name="responseId">The survey response identifier.</param>
        /// <returns>A PDF file if the response exists; otherwise not found.</returns>
        [HttpGet("{responseId:guid}/pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadPdf(Guid responseId)
        {
            SmeQuestionnaireResponse? response = await this.smeQuestionnaireDataService.GetResponseByIdAsync(responseId);
            if (response is null)
            {
                return this.NotFound();
            }

            SubjectMatterExpert? profile = await this.smeQuestionnaireDataService.GetSmeProfileByResponseIdAsync(responseId);
            List<SmeSkill> skills = await this.smeQuestionnaireDataService.GetSkillsByResponseIdAsync(responseId);
            List<SmeEquipment> equipment = await this.smeQuestionnaireDataService.GetEquipmentByResponseIdAsync(responseId);
            List<SmeSoftware> software = await this.smeQuestionnaireDataService.GetSoftwareByResponseIdAsync(responseId);
            SmeFacilityTourPreference? tourPreference = await this.smeQuestionnaireDataService.GetFacilityTourPreferenceByResponseIdAsync(responseId);

            byte[] pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(36);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .Column(column =>
                        {
                            column.Item().Text("SME Survey").Bold().FontSize(18);
                            column.Item().Text($"Response ID: {response.Id}");
                            column.Item().Text($"Status: {response.Status}");
                            column.Item().Text($"Started: {response.StartedOn.ToLocalTime():g}");
                            column.Item().Text($"Submitted: {FormatDateTime(response.SubmittedOn)}");
                        });

                    page.Content()
                        .PaddingTop(12)
                        .Column(column =>
                        {
                            column.Spacing(10);

                            column.Item().Text("Profile").Bold().FontSize(13);
                            column.Item().Text($"Name: {profile?.FullName ?? "-"}");
                            column.Item().Text($"Company: {profile?.Company ?? "-"}");
                            column.Item().Text($"Job Title: {profile?.JobTitle ?? "-"}");
                            column.Item().Text($"Email: {profile?.Email ?? "-"}");
                            column.Item().Text($"Phone: {profile?.Phone ?? "-"}");
                            column.Item().Text($"Website: {profile?.Website ?? "-"}");
                            column.Item().Text($"Location: {profile?.Location ?? "-"}");

                            column.Item().LineHorizontal(1);
                            column.Item().Text("Skills").Bold().FontSize(13);
                            if (skills.Count == 0)
                            {
                                column.Item().Text("- None");
                            }
                            else
                            {
                                foreach (SmeSkill skill in skills)
                                {
                                    string details = string.IsNullOrWhiteSpace(skill.Notes) ? "-" : skill.Notes;
                                    column.Item().Text($"- {skill.Name}: {details}");
                                }
                            }

                            column.Item().LineHorizontal(1);
                            column.Item().Text("Equipment").Bold().FontSize(13);
                            if (equipment.Count == 0)
                            {
                                column.Item().Text("- None");
                            }
                            else
                            {
                                foreach (SmeEquipment item in equipment)
                                {
                                    string details = string.IsNullOrWhiteSpace(item.Notes) ? "-" : item.Notes;
                                    column.Item().Text($"- {item.Name}: {details}");
                                }
                            }

                            column.Item().LineHorizontal(1);
                            column.Item().Text("Software").Bold().FontSize(13);
                            if (software.Count == 0)
                            {
                                column.Item().Text("- None");
                            }
                            else
                            {
                                foreach (SmeSoftware item in software)
                                {
                                    string details = string.IsNullOrWhiteSpace(item.Notes) ? "-" : item.Notes;
                                    column.Item().Text($"- {item.Name}: {details}");
                                }
                            }

                            column.Item().LineHorizontal(1);
                            column.Item().Text("Facility Tour").Bold().FontSize(13);
                            column.Item().Text($"Willing to offer tour: {FormatYesNo(tourPreference?.IsWillingToOfferTour)}");
                            column.Item().Text($"Details: {tourPreference?.Details ?? "-"}");

                            column.Item().Text("Availabilities").Bold();
                            List<SmeFacilityTourAvailability> availabilities = tourPreference?.Availabilities ?? new List<SmeFacilityTourAvailability>();
                            if (availabilities.Count == 0)
                            {
                                column.Item().Text("- None");
                            }
                            else
                            {
                                foreach (SmeFacilityTourAvailability availability in availabilities)
                                {
                                    string date = string.IsNullOrWhiteSpace(availability.Date) ? "-" : availability.Date;
                                    string start = string.IsNullOrWhiteSpace(availability.StartTime) ? "-" : availability.StartTime;
                                    string end = string.IsNullOrWhiteSpace(availability.EndTime) ? "-" : availability.EndTime;
                                    column.Item().Text($"- {date} ({start} - {end})");
                                }
                            }
                        });
                });
            }).GeneratePdf();

            string fileName = $"sme-survey-{responseId}.pdf";
            return this.File(pdfBytes, "application/pdf", fileName);
        }

        private static string FormatDateTime(DateTimeOffset? value)
        {
            return value.HasValue ? value.Value.ToLocalTime().ToString("g") : "-";
        }

        private static string FormatYesNo(bool? value)
        {
            if (!value.HasValue)
            {
                return "-";
            }

            return value.Value ? "Yes" : "No";
        }
    }
}
