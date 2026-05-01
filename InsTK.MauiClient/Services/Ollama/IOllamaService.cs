// Copyright (c) Robert Garner. All rights reserved.

using InsTK.MauiClient.Models;

namespace InsTK.MauiClient.Services.Ollama;

/// <summary>
/// Defines operations for checking local Ollama availability.
/// </summary>
public interface IOllamaService
{
    /// <summary>
    /// Gets the current Ollama connection status.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The current Ollama status snapshot.</returns>
    Task<OllamaConnectionStatus> GetStatusAsync(CancellationToken cancellationToken = default);
}
