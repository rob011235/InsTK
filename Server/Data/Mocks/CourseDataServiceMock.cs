// <copyright file="CourseDataServiceMock.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Server.Data.Mocks
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Interfaces;
    using Common.Models;

    /// <summary>
    /// Mock implementation of <see cref="ICoursesDataService"/> for testing and development purposes.
    /// Provides a static list of <see cref="Course"/> objects.
    /// </summary>
    public class CourseDataServiceMock : ICoursesDataService
    {
        /// <summary>
        /// The in-memory list of <see cref="Course"/> objects used by the mock service.
        /// </summary>
        private readonly List<Course> courses =
        [
            new Course
            {
                Id = Guid.NewGuid().ToString(),
                Number = "CIST 1220",
                Name = "Programming Fundamentals",
                Description = "An intro course for new programmers",
            },

            new Course
            {
                Id = Guid.NewGuid().ToString(),
                Number = "CSCI 1220",
                Name = "Python I",
                Description = "An introduction to programming with Python",
            },

            new Course
            {
                Id = Guid.NewGuid().ToString(),
                Number = "CIST 2284",
                Name = ".NET II",
                Description = "Web development with .NET",
            },
        ];

        /// <summary>
        /// Asynchronously retrieves all courses from the mock data source.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation.
        /// The task result contains a list of <see cref="Course"/> objects.
        /// </returns>
        public Task<List<Course>> GetAllAsync()
        {
            return Task.FromResult(this.courses.OrderBy(c => c.Number).ToList());
        }

        /// <summary>
        /// Asynchronously adds a new course to the collection.
        /// </summary>
        /// <param name="course">The course to add. The <see cref="Course.Id"/> property will be automatically assigned a new unique
        /// identifier.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task AddAsync(Course course)
        {
            course.Id = Guid.NewGuid().ToString();
            this.courses.Add(course);
            return Task.FromResult(course);
        }

        /// <summary>
        /// Asynchronously updates an existing course in the mock data source.
        /// </summary>
        /// <param name="course">The <see cref="Course"/> object containing updated course information.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous update operation.
        /// </returns>
        public Task UpdateAsync(Course course)
        {
            // Find the course to udate
            Course? courseToUpdate = this.courses.Where(c => c.Id == course.Id).FirstOrDefault();

            // If we found it update it
            if (courseToUpdate == null)
            {
                throw new ArgumentException("Course not found in database;");
            }

            courseToUpdate.Number = course.Number;
            courseToUpdate.Name = course.Name;
            courseToUpdate.Description = course.Description;
            return Task.FromResult(courseToUpdate);
        }

        /// <summary>
        /// Asynchronously deletes an existing course from the mock data source.
        /// </summary>
        /// <param name="course">The <see cref="Course"/> object to delete.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous delete operation.
        /// </returns>
        public Task DeleteAsync(string id)
        {
            Course? courseToDelete = this.courses.Where(c=>c.Id == id).FirstOrDefault();

            if (courseToDelete == null)
            {
                throw new ArgumentException("Course not found in database;");
            }

            this.courses.Remove(courseToDelete);
            return Task.FromResult(courseToDelete);
        }
    }
}
