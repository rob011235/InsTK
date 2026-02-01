// <copyright file="CoursesController.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Server.Controllers
{
    using Common.Interfaces;
    using Common.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")] // api/Courses
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly ICoursesDataService dataService;

        public CoursesController(ICoursesDataService dataService)
        {
            this.dataService = dataService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await dataService?.GetAllAsync());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task AddAsync(Course course)
        {
            await this.dataService.AddAsync(course);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task UpdateAsync(Course course)
        {
            await this.dataService.UpdateAsync(course);
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        [Route("{id}")]
        public async Task DeleteAsync(string id)
        {
            await this.dataService.DeleteAsync(id);
        }
    }
}
