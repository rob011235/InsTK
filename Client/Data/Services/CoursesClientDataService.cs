using Common.Interfaces;
using Common.Models;
using System.Net.Http.Json;

namespace Client.Data.Services
{
    public class CoursesClientDataService : ICoursesDataService
    {
        private readonly HttpClient http;

        public CoursesClientDataService(HttpClient http)
        {
            this.http = http;
        }

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

        public async Task AddAsync(Course course)
        {
            await this.http.PostAsJsonAsync<Course>("/api/Courses", course);
        }

        public async Task UpdateAsync(Course course)
        {
            await this.http.PutAsJsonAsync<Course>("/api/Courses", course);
        }

        public async Task DeleteAsync(string id)
        {
            await this.http.DeleteAsync($"/api/Courses/{id}");
        }
    }
}
