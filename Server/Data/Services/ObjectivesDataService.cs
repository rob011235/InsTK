// <copyright file="ObjectivesDataService.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Server.Data.Services
{
    using Server.Data;
    using Microsoft.EntityFrameworkCore;
    using Common.Interfaces;
    using Common.Models;

    /// <summary>
    /// Provides data access methods for <see cref="Objective"/> entities.
    /// </summary>
    public class ObjectivesDataService : IObjectivesDataService
    {
        /// <summary>
        /// The database context used for data operations.
        /// </summary>
        private ApplicationDbContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectivesDataService"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public ObjectivesDataService(ApplicationDbContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Gets all objectives ordered by objective number.
        /// </summary>
        /// <returns>A list of all <see cref="Objective"/> entities.</returns>
        public async Task<List<Objective>> GetAllAsync()
        {
            return await this.context.Objectives.OrderBy(obj => obj.ObjNumber).ToListAsync();
        }

        /// <summary>
        /// Adds a new objective to the database.
        /// </summary>
        /// <param name="objective">The objective to add.</param>
        public async Task AddAsync(Objective objective)
        {
            objective.Id = Guid.NewGuid().ToString();
            await this.context.AddAsync(objective);
            await this.context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing objective in the database.
        /// </summary>
        /// <param name="objective">The objective with updated values.</param>
        /// <exception cref="ArgumentException">Thrown if the objective is not found.</exception>
        public async Task UpdateAsync(Objective objective)
        {
            // Find the objective to update
            Objective? objectiveToUpdate = this.context.Objectives.Where(o => o.Id == objective.Id).FirstOrDefault();

            // If we found it update it
            if (objectiveToUpdate == null)
            {
                throw new ArgumentException("Objective not found in database;");
            }

            objectiveToUpdate.ObjNumber = objective.ObjNumber;
            objectiveToUpdate.ParentObj = objective.ParentObj;
            objectiveToUpdate.Title = objective.Title;
            objectiveToUpdate.Description = objective.Description;
            await this.context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes an objective from the database.
        /// </summary>
        /// <param name="objective">The objective to delete.</param>
        /// <exception cref="ArgumentException">Thrown if the objective is not found.</exception>
        public async Task DeleteAsync(string id)
        {
            Objective? objectiveToDelete = this.context.Objectives.Where(o => o.Id == id).FirstOrDefault();

            if (objectiveToDelete == null)
            {
                throw new ArgumentException("Course not found in database;");
            }

            this.context.Objectives.Remove(objectiveToDelete);
            await this.context.SaveChangesAsync();
        }
    }
}
