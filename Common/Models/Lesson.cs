// <copyright file="Lesson.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

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
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the objectives associated with the lesson.
    /// </summary>
    public List<Objective> Objectives { get; init; } = new List<Objective>();

    /// <summary>
    /// Gets the lesson description.
    /// </summary>
    public string Description { get; init; } = string.Empty;
}
