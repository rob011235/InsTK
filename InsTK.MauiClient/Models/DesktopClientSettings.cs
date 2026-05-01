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
    public string? BackendBaseUrl { get; set; } = "https://localhost:7016";

    /// <summary>
    /// Gets or sets the base URL for the local Ollama endpoint.
    /// </summary>
    public string OllamaBaseUrl { get; set; } = "http://127.0.0.1:11434";

    /// <summary>
    /// Gets or sets the browser channel to use for local Brightspace automation.
    /// </summary>
    public string BrightspaceBrowserChannel { get; set; } = "msedge";

    /// <summary>
    /// Gets or sets the Brightspace Quick Eval URL used to enumerate submissions.
    /// </summary>
    public string? BrightspaceQuickEvalUrl { get; set; }

    /// <summary>
    /// Gets or sets the local session-state file used for saved Brightspace sign-in state.
    /// </summary>
    public string BrightspaceStatePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the local JSON output path for merged Brightspace submission-map results.
    /// </summary>
    public string BrightspaceSubmissionMapOutPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the local root path used for grading workspaces and reports.
    /// </summary>
    public string WorkspaceRoot { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Ollama version required by the workstation policy.
    /// </summary>
    public string RequiredOllamaVersion { get; set; } = "0.22.1";

    /// <summary>
    /// Gets or sets the expected SHA-256 digest for the pinned Windows Ollama runtime zip.
    /// </summary>
    public string RequiredOllamaWindowsZipSha256 { get; set; } = "93c38a2ae97e4ab55c6d324e9cf62bc79408de85861045c34f4294c774d00c34";

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
