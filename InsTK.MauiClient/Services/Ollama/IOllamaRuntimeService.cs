// Copyright (c) Robert Garner. All rights reserved.

using InsTK.MauiClient.Models;

namespace InsTK.MauiClient.Services.Ollama;

/// <summary>
/// Defines higher-level workstation policy checks and operations for the managed Ollama runtime.
/// </summary>
public interface IOllamaRuntimeService
{
    /// <summary>
    /// Occurs when a managed Ollama runtime operation reports activity or progress.
    /// </summary>
    event EventHandler<OllamaRuntimeActivityEventArgs>? ActivityReported;

    /// <summary>
    /// Evaluates the current workstation readiness for the configured managed Ollama runtime.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The evaluated Ollama runtime status.</returns>
    Task<OllamaRuntimeStatus> GetRuntimeStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads and installs or updates the managed Ollama runtime required by workstation policy.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The runtime installation result.</returns>
    Task<OllamaRuntimeOperationResult> InstallOrUpdateManagedRuntimeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts the managed Ollama runtime using the configured local model storage path.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The runtime start result.</returns>
    Task<OllamaRuntimeOperationResult> StartManagedRuntimeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads the required grading models defined by workstation policy.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The model download result.</returns>
    Task<OllamaRuntimeOperationResult> EnsureRequiredModelsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a specific Ollama model into the configured local model store.
    /// </summary>
    /// <param name="model">The model name to download.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The model download result.</returns>
    Task<OllamaRuntimeOperationResult> EnsureModelAsync(string model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a specific Ollama model from the configured local model store.
    /// </summary>
    /// <param name="model">The model name to remove.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The model removal result.</returns>
    Task<OllamaRuntimeOperationResult> RemoveModelAsync(string model, CancellationToken cancellationToken = default);
}
