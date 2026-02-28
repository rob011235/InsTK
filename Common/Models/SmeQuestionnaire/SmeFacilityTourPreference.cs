// <copyright file="SmeFacilityTourPreference.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Common.Models.SmeQuestionnaire
{
    /// <summary>
    /// Represents an SME's willingness to provide a facility tour.
    /// </summary>
    public class SmeFacilityTourPreference
    {
        /// <summary>
        /// Gets or sets a value indicating whether the SME is willing to provide a tour.
        /// </summary>
        public bool? IsWillingToOfferTour { get; set; }

        /// <summary>
        /// Gets or sets optional details related to the tour availability.
        /// </summary>
        public string? Details { get; set; }
    }
}
