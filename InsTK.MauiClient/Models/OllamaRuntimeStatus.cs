// Copyright (c) Robert Garner. All rights reserved.

namespace InsTK.MauiClient.Models;

/// <summary>
/// Represents the evaluated workstation setup status for the managed Ollama runtime and models.
/// </summary>
/// <param name="State">The current workstation runtime state.</param>
/// <param name="ManagedExecutablePath">The expected managed Ollama executable path.</param>
/// <param name="RequiredVersion">The version required by workstation policy.</param>
/// <param name="DetectedVersion">The version detected from the managed runtime, if any.</param>
/// <param name="PrimaryModel">The configured primary model.</param>
/// <param name="FallbackModel">The configured fallback model.</param>
/// <param name="HasPrimaryModel">A value indicating whether the primary model is installed.</param>
/// <param name="HasFallbackModel">A value indicating whether the fallback model is installed.</param>
/// <param name="ModelsRoot">The configured Ollama models path.</param>
/// <param name="Summary">A human-readable summary of the current state.</param>
/// <param name="ErrorMessage">The last relevant error message, if any.</param>
public sealed record OllamaRuntimeStatus(
    OllamaRuntimeState State,
    string ManagedExecutablePath,
    string RequiredVersion,
    string? DetectedVersion,
    string PrimaryModel,
    string FallbackModel,
    bool HasPrimaryModel,
    bool HasFallbackModel,
    string ModelsRoot,
    string Summary,
    string? ErrorMessage);
