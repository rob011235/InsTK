// <copyright file="CoursesController.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Server.Controllers
{
    using Common.Interfaces;
    using Common.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// API controller for managing course resources.
    /// </summary>
    [Route("api/[controller]")] // api/Courses
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly ICoursesDataService dataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoursesController"/> class.
        /// </summary>
        /// <param name="dataService">The course data service.</param>
        public CoursesController(ICoursesDataService dataService)
        {
            this.dataService = dataService;
        }

        /// <summary>
        /// Gets all courses.
        /// </summary>
        /// <returns>The HTTP action result containing the list of courses.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            return this.Ok(await this.dataService.GetAllAsync());
        }

        /// <summary>
        /// Adds a new course.
        /// </summary>
        /// <param name="course">The course to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task AddAsync(Course course)
        {
            await this.dataService.AddAsync(course);
        }

        /// <summary>
        /// Updates an existing course.
        /// </summary>
        /// <param name="course">The course to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task UpdateAsync(Course course)
        {
            await this.dataService.UpdateAsync(course);
        }

        /// <summary>
        /// Deletes a course by identifier.
        /// </summary>
        /// <param name="id">The course identifier.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        [Route("{id}")]
        public async Task DeleteAsync(string id)
        {
            await this.dataService.DeleteAsync(id);
        }
    }
}
