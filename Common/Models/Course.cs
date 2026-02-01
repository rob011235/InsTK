// <copyright file="Course.cs" company="Rob Garner (rgarner7@cnm.edu)">
// Copyright (c) Rob Garner (rgarner7@cnm.edu). All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models
{
    /// <summary>
    /// Represents a course with its identifying information and description.
    /// </summary>
    public class Course
    {
        /// <summary>
        /// Gets or sets the unique identifier for the course.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the course number (example CIST 1220).
        /// </summary>
        public string? Number { get; set; }

        /// <summary>
        /// Gets or sets the name of the course.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the course.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the list of objectives associated with the course.
        /// </summary>
        public List<Objective>? Objectives { get; set; }
    }
}
