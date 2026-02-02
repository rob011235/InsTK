// <copyright file="CoursesClientDataService.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Client.Data.Services
{
    using System.Net.Http.Json;
    using Common.Interfaces;
    using Common.Models;

    /// <summary>
    /// Provides client-side CRUD operations for courses via HTTP requests.
    /// </summary>
    public class CoursesClientDataService : ICoursesDataService
    {
        private readonly HttpClient http;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoursesClientDataService"/> class.
        /// </summary>
        /// <param name="http">The HTTP client used to send requests to the API.</param>
        public CoursesClientDataService(HttpClient http)
        {
            this.http = http;
        }

        /// <inheritdoc/>
        public async Task<List<Course>> GetAllAsync()
        {
            var courses = await this.http.GetFromJsonAsync<List<Course>>("/api/Courses");
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
        public async Task AddAsync(Course course)
        {
            await this.http.PostAsJsonAsync<Course>("/api/Courses", course);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(Course course)
        {
            await this.http.PutAsJsonAsync<Course>("/api/Courses", course);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string id)
        {
            await this.http.DeleteAsync($"/api/Courses/{id}");
        }
    }
}
