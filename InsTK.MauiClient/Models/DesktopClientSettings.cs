// Copyright (c) Robert Garner. All rights reserved.

namespace InsTK.MauiClient.Models;

/// <summary>
/// Stores persisted workstation settings used by the MAUI client.
/// </summary>
public sealed class DesktopClientSettings
{
    /// <summary>
    /// Gets or sets the base URL for the InsTK web backend.
    /// </summary>
    public string? BackendBaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the base URL for the local Ollama endpoint.
    /// </summary>
    public string OllamaBaseUrl { get; set; } = "http://127.0.0.1:11434";

    /// <summary>
    /// Gets or sets the local root path used for grading workspaces and reports.
    /// </summary>
    public string WorkspaceRoot { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Ollama version required by the workstation policy.
    /// </summary>
    public string RequiredOllamaVersion { get; set; } = "0.22.1";

    /// <summary>
    /// Gets or sets the preferred primary Ollama model used for grading.
    /// </summary>
    public string PrimaryOllamaModel { get; set; } = "qwen3-coder:30b";

    /// <summary>
    /// Gets or sets the fallback Ollama model used when the primary profile is unavailable.
    /// </summary>
    public string FallbackOllamaModel { get; set; } = "deepseek-coder:6.7b";

    /// <summary>
    /// Gets or sets the root folder where the managed Ollama runtime should be placed.
    /// </summary>
    public string ManagedOllamaRoot { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the folder where Ollama models should be stored.
    /// </summary>
    public string OllamaModelsRoot { get; set; } = string.Empty;
}
