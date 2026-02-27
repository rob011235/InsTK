// <copyright file="IObjectivesDataService.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Common.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Models;

    /// <summary>
    /// Provides methods for managing <see cref="Objective"/> entities in the data store.
    /// </summary>
    public interface IObjectivesDataService
    {
        /// <summary>
        /// Asynchronously retrieves all objectives.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a list of <see cref="Objective"/> objects.
        /// </returns>
        Task<List<Objective>> GetAllAsync();

        /// <summary>
        /// Asynchronously adds the specified <see cref="Objective"/> entity to the data store.
        /// </summary>
        /// <param name="objective">The <see cref="Objective"/> entity to add.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the result of the asynchronous operation.
        /// </returns>
        Task AddAsync(Objective objective);

        /// <summary>
        /// Asynchronously updates the specified <see cref="Objective"/> entity.
        /// </summary>
        /// <param name="objective">The <see cref="Objective"/> to update.</param>
        /// <returns>
        /// A task that represents the asynchronous update operation.
        /// </returns>
        Task UpdateAsync(Objective objective);

        /// <summary>
        /// Asynchronously deletes the specified <see cref="Objective"/> entity.
        /// </summary>
        /// <param name="objective">The <see cref="Objective"/> entity to delete from the data store.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the result of the asynchronous operation.
        /// </returns>
        Task DeleteAsync(string id);
    }
}
