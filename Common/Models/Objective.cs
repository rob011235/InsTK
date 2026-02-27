// <copyright file="Objective.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Common.Models
{
    /// <summary>
    /// Represents an objective with identification, hierarchy, title, and description.
    /// </summary>
    public class Objective
    {
        /// <summary>
        /// Gets or sets the unique identifier for the objective.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the objective number.
        /// </summary>
        public string? ObjNumber { get; set; }

        /// <summary>
        /// Gets or sets the parent objective identifier.
        /// </summary>
        public string? ParentObj { get; set; }

        /// <summary>
        /// Gets or sets the title of the objective.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the objective.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the list of courses associated with the entity.
        /// </summary>
        public List<Course>? Courses { get; set; }
    }
}
