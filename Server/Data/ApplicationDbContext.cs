// <copyright file="ApplicationDbContext.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Server.Data
{
    using Common.Models;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Represents the application's database context, including Identity and custom entities.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the collection of courses in the database.
        /// </summary>
        public DbSet<Course> Courses { get; set; }

        /// <summary>
        /// Gets or sets the collection of objectives in the database.
        /// </summary>
        public DbSet<Objective> Objectives { get; set; }
    }
}