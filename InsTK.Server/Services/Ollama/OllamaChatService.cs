// Copyright (c) Robert Garner. All rights reserved.

using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace InsTK.Server.Services.Ollama;

/// <summary>
/// Uses the Ollama HTTP API to generate responses for submitted prompts.
/// </summary>
/// <param name="httpClient">The HTTP client used to call Ollama.</param>
/// <param name="options">The configured Ollama connection settings.</param>
public sealed class OllamaChatService(HttpClient httpClient, IOptions<OllamaChatOptions> options) : IOllamaChatService
{
    private readonly HttpClient httpClient = httpClient;
    private readonly OllamaChatOptions options = Normalize(options.Value);

    /// <inheritdoc />
    public Task<OllamaChatConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new OllamaChatConfiguration(options.BaseUrl, options.Model));
    }

    /// <inheritdoc />
    public async Task<OllamaGenerateResult> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);

        using var response = await httpClient.PostAsJsonAsync(
            "api/generate",
            new OllamaGenerateRequest(options.Model, prompt, false),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var details = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Ollama request failed with HTTP {(int)response.StatusCode} ({response.ReasonPhrase}). {details}".Trim());
        }

        var payload = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(cancellationToken: cancellationToken);

        if (payload is null || string.IsNullOrWhiteSpace(payload.Response))
        {
            throw new InvalidOperationException("Ollama returned an empty response.");
        }

        return new OllamaGenerateResult(payload.Response.Trim());
    }

    private static OllamaChatOptions Normalize(OllamaChatOptions value)
    {
        return new OllamaChatOptions
        {
            BaseUrl = string.IsNullOrWhiteSpace(value.BaseUrl) ? "http://127.0.0.1:11434" : value.BaseUrl.Trim().TrimEnd('/'),
            Model = string.IsNullOrWhiteSpace(value.Model) ? "qwen3-coder:30b" : value.Model.Trim()
        };
    }

    private sealed record OllamaGenerateRequest(string Model, string Prompt, bool Stream);

    private sealed class OllamaGenerateResponse
    {
        [JsonPropertyName("response")]
        public string? Response { get; init; }
    }
}
