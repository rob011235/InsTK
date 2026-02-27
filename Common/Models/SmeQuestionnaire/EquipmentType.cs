// <copyright file="EquipmentType.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Common.Models.SmeQuestionnaire
{
    /// <summary>
    /// Defines equipment categories.
    /// </summary>
    public enum EquipmentType
    {
        /// <summary>
        /// Physical hardware tools and devices.
        /// </summary>
        Hardware,

        /// <summary>
        /// Measurement or diagnostic instruments.
        /// </summary>
        Instrument,

        /// <summary>
        /// Facility resources such as labs or specialized spaces.
        /// </summary>
        Facility,

        /// <summary>
        /// Safety equipment used to perform work safely.
        /// </summary>
        Safety,
    }
}
