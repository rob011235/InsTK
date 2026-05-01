// Copyright (c) Robert Garner. All rights reserved.

namespace InsTK.MauiClient.Models;

/// <summary>
/// Represents the current workstation hardware profile used for Ollama suitability checks.
/// </summary>
/// <param name="OperatingSystem">The current operating system description.</param>
/// <param name="ProcessorCount">The number of logical processors available to the process.</param>
/// <param name="TotalMemoryBytes">The total physical memory installed on the machine.</param>
/// <param name="AvailableMemoryBytes">The currently available physical memory.</param>
/// <param name="RecommendedPrimaryModel">The recommended primary Ollama model for the current machine profile.</param>
/// <param name="RecommendationSummary">A human-readable explanation of the recommendation.</param>
public sealed record WorkstationProfile(
    string OperatingSystem,
    int ProcessorCount,
    ulong TotalMemoryBytes,
    ulong AvailableMemoryBytes,
    string RecommendedPrimaryModel,
    string RecommendationSummary);
