// Copyright (c) Robert Garner. All rights reserved.

namespace InsTK.MauiClient.Services.Ollama;

/// <summary>
/// Sends prompts to the configured Ollama endpoint and returns generated text.
/// </summary>
public interface IOllamaPromptService
{
    /// <summary>
    /// Gets the active Ollama prompt configuration.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The active prompt configuration.</returns>
    Task<OllamaPromptConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a prompt to Ollama and returns the generated response.
    /// </summary>
    /// <param name="prompt">The prompt to send.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The generated response and model metadata.</returns>
    Task<OllamaPromptResult> GenerateAsync(string prompt, CancellationToken cancellationToken = default);
}
