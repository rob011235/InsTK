// <copyright file="ObjectivesController.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Server.Controllers
{
    using Common.Interfaces;
    using Common.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// API controller for managing objectives.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ObjectivesController"/> class.
    /// </remarks>
    /// <param name="dataService">The objectives data service.</param>
    [Route("api/[controller]")] // api/Courses
    [ApiController]
    public class ObjectivesController(IObjectivesDataService dataService) : ControllerBase
    {
        private readonly IObjectivesDataService dataService = dataService;

        /// <summary>
        /// Gets all objectives.
        /// </summary>
        /// <returns>A list of objectives.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await this.dataService.GetAllAsync();
            if (result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }

        /// <summary>
        /// Adds a new objective.
        /// </summary>
        /// <param name="objective">The objective to add.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task AddAsync(Objective objective)
        {
            await this.dataService.AddAsync(objective);
        }

        /// <summary>
        /// Updates an existing objective.
        /// </summary>
        /// <param name="objective">The objective to update.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task UpdateAsync(Objective objective)
        {
            await this.dataService.UpdateAsync(objective);
        }

        /// <summary>
        /// Deletes an objective by id.
        /// </summary>
        /// <param name="id">The id of the objective to delete.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        [Route("{id}")]
        public async Task DeleteAsync(string id)
        {
            await this.dataService.DeleteAsync(id);
        }
    }
}
