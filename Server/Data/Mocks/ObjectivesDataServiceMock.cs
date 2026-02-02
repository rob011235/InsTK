// <copyright file="ObjectivesDataServiceMock.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Server.Data.Mocks
{
    using Common.Interfaces;
    using Common.Models;

    /// <summary>
    /// Mock implementation of <see cref="IObjectivesDataService"/> for testing and development purposes.
    /// Provides in-memory storage and management of <see cref="Objective"/> entities.
    /// </summary>
    public class ObjectivesDataServiceMock : IObjectivesDataService
    {
        /// <summary>
        /// In-memory list of <see cref="Objective"/> entities.
        /// </summary>
        private readonly List<Objective> objectives =
        [
            new Objective
            {
                Id = Guid.NewGuid().ToString(),
                ObjNumber = "1",
                ParentObj = null,
                Title = "Understand Python Syntax and Semantics",
                Description = "Demonstrate knowledge of basic Python syntax, keywords, and program structure.",
            },
            new Objective
            {
                Id = Guid.NewGuid().ToString(),
                ObjNumber = "2",
                ParentObj = null,
                Title = "Work with Data Types and Variables",
                Description = "Use Python data types such as integers, floats, strings, and lists, and manipulate variables.",
            },
            new Objective
            {
                Id = Guid.NewGuid().ToString(),
                ObjNumber = "3",
                ParentObj = null,
                Title = "Implement Control Flow",
                Description = "Apply conditional statements and loops to control the flow of Python programs.",
            },
            new Objective
            {
                Id = Guid.NewGuid().ToString(),
                ObjNumber = "4",
                ParentObj = null,
                Title = "Define and Use Functions",
                Description = "Create and invoke functions to organize code and promote reuse in Python.",
            },
            new Objective
            {
                Id = Guid.NewGuid().ToString(),
                ObjNumber = "5",
                ParentObj = null,
                Title = "Handle Errors and Exceptions",
                Description = "Utilize Python exception handling to manage errors and ensure program stability.",
            },
        ];

        /// <summary>
        /// Asynchronously retrieves all <see cref="Objective"/> entities.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a list of <see cref="Objective"/> objects.
        /// </returns>
        public Task<List<Objective>> GetAllAsync()
        {
            return Task.FromResult(this.objectives.OrderBy(obj => obj.ObjNumber).ToList());
        }

        /// <summary>
        /// Asynchronously adds the specified <see cref="Objective"/> entity to the in-memory list.
        /// </summary>
        /// <param name="objective">The <see cref="Objective"/> entity to add.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the result of the asynchronous operation.
        /// </returns>
        public Task AddAsync(Objective objective)
        {
            objective.Id = Guid.NewGuid().ToString();
            this.objectives.Add(objective);
            return Task.FromResult(objective);
        }

        /// <summary>
        /// Asynchronously updates the specified <see cref="Objective"/> entity in the in-memory list.
        /// </summary>
        /// <param name="objective">The <see cref="Objective"/> to update.</param>
        /// <returns>
        /// A task that represents the asynchronous update operation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the objective is not found in the database.</exception>
        public Task UpdateAsync(Objective objective)
        {
            // Find the objective to update
            Objective? objectiveToUpdate = this.objectives.Where(o => o.Id == objective.Id).FirstOrDefault() ?? throw new ArgumentException("Objective not found in database;");
            objectiveToUpdate.ObjNumber = objective.ObjNumber;
            objectiveToUpdate.ParentObj = objective.ParentObj;
            objectiveToUpdate.Title = objective.Title;
            objectiveToUpdate.Description = objective.Description;
            return Task.FromResult(objectiveToUpdate);
        }

        /// <summary>
        /// Asynchronously deletes the specified <see cref="Objective"/> entity from the in-memory list.
        /// </summary>
        /// <param name="id">The unique identifier of the <see cref="Objective"/> entity to delete from the data store.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the result of the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the objective is not found in the database.</exception>
        public Task DeleteAsync(string id)
        {
            Objective? objectiveToDelete = this.objectives.Where(o => o.Id == id).FirstOrDefault() ?? throw new ArgumentException("Course not found in database;");
            this.objectives.Remove(objectiveToDelete);
            return Task.FromResult(objectiveToDelete);
        }
    }
}
