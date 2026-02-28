// <copyright file="SmeFacilityTourAvailability.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Common.Models.SmeQuestionnaire
{
    /// <summary>
    /// Represents a single date/time availability slot for a facility tour.
    /// </summary>
    public class SmeFacilityTourAvailability
    {
        /// <summary>
        /// Gets or sets the available date in yyyy-MM-dd format.
        /// </summary>
        public string? Date { get; set; }

        /// <summary>
        /// Gets or sets the available start time in HH:mm format.
        /// </summary>
        public string? StartTime { get; set; }

        /// <summary>
        /// Gets or sets the available end time in HH:mm format.
        /// </summary>
        public string? EndTime { get; set; }
    }
}
