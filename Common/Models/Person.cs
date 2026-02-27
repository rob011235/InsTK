// <copyright file="Person.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace MyApp.Models;

/// <summary>
/// Represents a person with a name and age.
/// </summary>
public class Person
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Person"/> class.
    /// </summary>
    /// <param name="firstName">The first name.</param>
    /// <param name="age">The age in years.</param>
    public Person(string firstName, int age)
    {
        this.FirstName = firstName;
        this.Age = age;
    }
    
    /// <summary>
    /// Gets the first name.
    /// </summary>
    public string FirstName { get; init; }

    /// <summary>
    /// Gets the age in years.
    /// </summary>
    public int Age { get; init; }
}
