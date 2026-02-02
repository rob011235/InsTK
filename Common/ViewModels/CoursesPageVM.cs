// <copyright file="CoursesPageVM.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Common.ViewModels
{
    using System.Collections.Generic;
    using Common.Models;

    /// <summary>
    /// ViewModel for the Courses page, containing a list of courses and a selected course.
    /// </summary>
    public class CoursesPageVM
    {
        /// <summary>
        /// Gets or sets the list of courses.
        /// </summary>
        public List<Course>? Courses { get; set; }

        /// <summary>
        /// Gets or sets the selected course.
        /// </summary>
        public Course? Course { get; set; }
    }
}
