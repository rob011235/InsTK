// <copyright file="ObjectivesClientDataService.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Client.Data.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Common.Interfaces;
    using Common.Models;

    /// <summary>
    /// Provides client-side data service operations for objectives.
    /// </summary>
    public class ObjectivesClientDataService : IObjectivesDataService
    {
        private readonly HttpClient http;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectivesClientDataService"/> class.
        /// </summary>
        /// <param name="http">The HTTP client used for API requests.</param>
        public ObjectivesClientDataService(HttpClient http)
        {
            this.http = http;
        }

        /// <inheritdoc/>
        public async Task<List<Objective>> GetAllAsync()
        {
            var courses = await this.http.GetFromJsonAsync<List<Objective>>("/api/Objectives");
            if (courses == null)
            {
                return [];
            }
            else
            {
                return courses;
            }
        }

        /// <inheritdoc/>
        public async Task AddAsync(Objective objective)
        {
            await this.http.PostAsJsonAsync<Objective>("/api/Objectives", objective);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(Objective objective)
        {
            await this.http.PutAsJsonAsync<Objective>("/api/Objectives", objective);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string id)
        {
            await this.http.DeleteAsync($"/api/Objectives/{id}");
        }
    }
}