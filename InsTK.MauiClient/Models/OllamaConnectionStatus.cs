// Copyright (c) Robert Garner. All rights reserved.

namespace InsTK.MauiClient.Models;

/// <summary>
/// Represents the current Ollama connectivity state for the workstation.
/// </summary>
/// <param name="BaseUrl">The configured Ollama base URL.</param>
/// <param name="IsConfigured">A value indicating whether Ollama has been configured.</param>
/// <param name="IsReachable">A value indicating whether Ollama was reachable.</param>
/// <param name="ModelCount">The number of local models reported by Ollama.</param>
/// <param name="Summary">A human-readable summary of the current state.</param>
/// <param name="ErrorMessage">The last connection error, if any.</param>
public sealed record OllamaConnectionStatus(
    string BaseUrl,
    bool IsConfigured,
    bool IsReachable,
    int ModelCount,
    string Summary,
    string? ErrorMessage);
