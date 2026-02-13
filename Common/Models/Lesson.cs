using System;
using System.Collections.Generic;
using Common.Models;

public class Lesson
{
    public Guid Id { get; init; }
    public string Title { get; init; }
    public List<Objective> Objectives { get; init; }
    public string Description { get; init; }
}
