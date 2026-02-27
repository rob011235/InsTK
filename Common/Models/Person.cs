namespace MyApp.Models;

/// <summary>
/// Represents a person with a name and age.
/// </summary>
public class Person
{
    /// <summary>
    /// Gets the first name.
    /// </summary>
    public string FirstName { get; init; }

    /// <summary>
    /// Gets the age in years.
    /// </summary>
    public int Age { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Person"/> class.
    /// </summary>
    /// <param name="firstName">The first name.</param>
    /// <param name="age">The age in years.</param>
    public Person(string firstName, int age)
    {
        FirstName = firstName;
        Age = age;
    }
}
