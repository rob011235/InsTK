// Copyright (c) Robert Garner. All rights reserved.

namespace InsTK.Server.Services.Ollama;

/// <summary>
/// Sends prompts to a configured Ollama instance and returns generated responses.
/// </summary>
public interface IOllamaChatService
{
    /// <summary>
    /// Gets the effective Ollama connection configuration.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The configured base URL and model information.</returns>
    Task<OllamaChatConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a prompt to Ollama and returns the generated response text.
    /// </summary>
    /// <param name="prompt">The prompt to submit.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The generated response payload.</returns>
    Task<OllamaGenerateResult> GenerateAsync(string prompt, CancellationToken cancellationToken = default);
}
