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
}
