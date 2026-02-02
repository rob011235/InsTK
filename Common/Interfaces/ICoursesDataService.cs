// <copyright file="ICoursesDataService.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Common.Interfaces
{
    using Common.Models;

    /// <summary>
    /// Provides data access methods for <see cref="Course"/> entities.
    /// </summary>
    public interface ICoursesDataService
    {
        /// <summary>
        /// Asynchronously retrieves all courses.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Course"/> objects.</returns>
        public Task<List<Course>> GetAllAsync();

        /// <summary>
        /// Asynchronously adds the specified <see cref="Course"/> entity to the data store.
        /// </summary>
        /// <param name="course">The <see cref="Course"/> entity to add.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public Task AddAsync(Course course);

        /// <summary>
        /// Asynchronously updates the specified <see cref="Course"/> entity.
        /// </summary>
        /// <param name="course">The <see cref="Course"/> to update.</param>
        /// <returns>A task that represents the asynchronous update operation.</returns>
        public Task UpdateAsync(Course course);

        /// <summary>
        /// Asynchronously deletes the specified <see cref="Course"/> entity.
        /// </summary>
        /// <param name="id">The identifier of the <see cref="Course"/> entity to delete from the data store.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public Task DeleteAsync(string id);
    }
}
