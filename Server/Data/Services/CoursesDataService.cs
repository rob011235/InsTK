// <copyright file="CoursesDataService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Data.Services
{
    using Common.Interfaces;
    using Common.Models;
    using Microsoft.EntityFrameworkCore;
    using Server.Data;

    /// <summary>
    /// Provides data operations for <see cref="Course"/> entities.
    /// </summary>
    public class CoursesDataService : ICoursesDataService
    {
        private readonly ApplicationDbContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoursesDataService"/> class.
        /// </summary>
        /// <param name="context">The database context to use for data operations.</param>
        public CoursesDataService(ApplicationDbContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Gets all courses asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of courses.</returns>
        public async Task<List<Course>> GetAllAsync()
        {
            return await this.context.Courses.ToListAsync();
        }

        /// <summary>
        /// Adds a new course asynchronously.
        /// </summary>
        /// <param name="course">The course to add.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task AddAsync(Course course)
        {
            course.Id = Guid.NewGuid().ToString();
            this.context.Courses.Add(course);
            await this.context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing course asynchronously.
        /// </summary>
        /// <param name="course">The course to update.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task UpdateAsync(Course course)
        {
            // Find the one in the database
            var curentCourse = await this.context.Courses.Where(c => c.Id == course.Id).FirstOrDefaultAsync();

            if (curentCourse == null)
            {
                throw new InvalidOperationException("Course not found.");
            }

            // Update it from the values in the parameter course
            curentCourse.Number = course.Number;
            curentCourse.Name = course.Name;
            curentCourse.Description = course.Description;

            // Save changes.
            await this.context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a course asynchronously.
        /// </summary>
        /// <param name="id">The ID of the course to delete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task DeleteAsync(string id)
        {
            // Find the one in the database
            var curentCourse = await this.context.Courses.Where(c => c.Id == id).FirstOrDefaultAsync();

            if (curentCourse == null)
            {
                throw new InvalidOperationException("Course not found.");
            }

            this.context.Courses.Remove(curentCourse);
            await this.context.SaveChangesAsync();
        }
    }
}
