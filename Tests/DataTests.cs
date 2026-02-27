// <copyright file="DataTests.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Tests
{
    using System.Threading.Tasks;
    using Common.Models;
    using Server.Data.Mocks;

    /// <summary>
    /// Contains unit tests for data service mocks related to courses.
    /// </summary>
    [TestClass]
    public sealed class DataTests
    {
        /// <summary>
        /// Tests that <see cref="CourseDataServiceMock.GetAllAsync"/> returns a non-null list of <see cref="Course"/>.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [TestMethod]
        public async Task CoursesDataServiceMockReturnsAListOfCoursesAsync()
        {
            var classUnderTest = new CourseDataServiceMock();
            var actualResult = await classUnderTest.GetAllAsync();
            Assert.IsNotNull(actualResult);
            Assert.IsInstanceOfType<List<Course>>(actualResult);
        }
    }
}
