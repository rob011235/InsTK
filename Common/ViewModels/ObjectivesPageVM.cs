// <copyright file="ObjectivesPageVM.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Common.ViewModels
{
    using System.Collections.Generic;
    using Common.Models;

    /// <summary>
    /// ViewModel for the Objectives page, containing a list of objectives and a selected objective.
    /// </summary>
    public class ObjectivesPageVM
    {
        /// <summary>
        /// Gets or sets the list of objectives.
        /// </summary>
        public List<Objective>? Objectives { get; set; }

        /// <summary>
        /// Gets or sets the currently selected objective.
        /// </summary>
        public Objective? Objective { get; set; }
    }
}