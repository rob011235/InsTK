// <copyright file="SmeSoftware.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Common.Models.SmeQuestionnaire
{
    using System;

    /// <summary>
    /// Represents a normalized software item captured from an SME response.
    /// </summary>
    public class SmeSoftware
    {
        /// <summary>
        /// Gets or sets the software identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the parent response identifier.
        /// </summary>
        public Guid ResponseId { get; set; }

        /// <summary>
        /// Gets or sets the software name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the vendor.
        /// </summary>
        public string? Vendor { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        public string? Version { get; set; }

        /// <summary>
        /// Gets or sets the software category.
        /// </summary>
        public SoftwareCategory Category { get; set; }

        /// <summary>
        /// Gets or sets usage frequency.
        /// </summary>
        public UsageFrequency Frequency { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this software is required for the job.
        /// </summary>
        public bool RequiredForJob { get; set; }

        /// <summary>
        /// Gets or sets optional notes.
        /// </summary>
        public string? Notes { get; set; }
    }
}
