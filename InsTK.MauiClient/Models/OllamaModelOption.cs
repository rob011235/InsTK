// Copyright (c) Robert Garner. All rights reserved.

namespace InsTK.MauiClient.Models;

/// <summary>
/// Represents an Ollama model option that may be suitable for the current workstation.
/// </summary>
/// <param name="Name">The exact Ollama model name.</param>
/// <param name="DisplayName">The user-facing display label.</param>
/// <param name="DownloadSizeLabel">The approximate download size label.</param>
/// <param name="MinimumRecommendedAvailableMemoryBytes">The minimum currently available memory recommended for this model.</param>
/// <param name="Notes">A short capability summary for the model.</param>
public sealed record OllamaModelOption(
    string Name,
    string DisplayName,
    string DownloadSizeLabel,
    ulong MinimumRecommendedAvailableMemoryBytes,
    string Notes);
