using Common.Interfaces;
using Common.Models;
using System.Net.Http.Json;

namespace Client.Data.Services
{
    /// <summary>
    /// Client-side implementation for course data operations.
    /// </summary>
    public class CoursesClientDataService : ICoursesDataService
    {
        private readonly HttpClient http;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoursesClientDataService"/> class.
        /// </summary>
        /// <param name="http">The HTTP client used to call the courses API.</param>
        public CoursesClientDataService(HttpClient http)
        {
            this.http = http;
        }

        /// <summary>
        /// Gets all courses from the API.
        /// </summary>
        /// <returns>A list of courses.</returns>
        public async Task<List<Course>> GetAllAsync()
        {
            var courses = await this.http.GetFromJsonAsync<List<Course>>("/api/Courses");
            if (courses == null)
            {
                return new List<Course>();
            }
            else
            {
                return courses;
            }
        }

        /// <summary>
        /// Adds a course through the API.
        /// </summary>
        /// <param name="course">The course to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddAsync(Course course)
        {
            await this.http.PostAsJsonAsync<Course>("/api/Courses", course);
        }

        /// <summary>
        /// Updates a course through the API.
        /// </summary>
        /// <param name="course">The course to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateAsync(Course course)
        {
            await this.http.PutAsJsonAsync<Course>("/api/Courses", course);
        }

        /// <summary>
        /// Deletes a course through the API.
        /// </summary>
        /// <param name="id">The course identifier.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteAsync(string id)
        {
            await this.http.DeleteAsync($"/api/Courses/{id}");
        }
    }
}
