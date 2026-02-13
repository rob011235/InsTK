namespace MyApp.Models;

public class Person
{
    public string FirstName { get; init; }
    public int Age { get; init; }

    public Person(string firstName, int age)
    {
        FirstName = firstName;
        Age = age;
    }
}
