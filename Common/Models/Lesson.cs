using System;
using System.Collections.Generic;
using Common.Models;

/// <summary>
/// Represents a lesson with objectives and descriptive metadata.
/// </summary>
public class Lesson
{
    /// <summary>
    /// Gets the lesson identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the lesson title.
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// Gets the objectives associated with the lesson.
    /// </summary>
    public List<Objective> Objectives { get; init; }

    /// <summary>
    /// Gets the lesson description.
    /// </summary>
    public string Description { get; init; }
}
