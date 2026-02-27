// <copyright file="SmeEquipment.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Common.Models.SmeQuestionnaire
{
    using System;

    /// <summary>
    /// Represents a normalized equipment item captured from an SME response.
    /// </summary>
    public class SmeEquipment
    {
        /// <summary>
        /// Gets or sets the equipment identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the parent response identifier.
        /// </summary>
        public Guid ResponseId { get; set; }

        /// <summary>
        /// Gets or sets the equipment name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the equipment type.
        /// </summary>
        public EquipmentType Type { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer.
        /// </summary>
        public string? Manufacturer { get; set; }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Gets or sets version or specification details.
        /// </summary>
        public string? VersionOrSpec { get; set; }

        /// <summary>
        /// Gets or sets usage frequency.
        /// </summary>
        public UsageFrequency Frequency { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this item is required for the job.
        /// </summary>
        public bool RequiredForJob { get; set; }

        /// <summary>
        /// Gets or sets optional notes.
        /// </summary>
        public string? Notes { get; set; }
    }
}
