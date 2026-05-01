// Copyright (c) Robert Garner. All rights reserved.

using System.Text.Json;
using InsTK.MauiClient.Models;
using InsTK.MauiClient.Services.Settings;

namespace InsTK.MauiClient.Services.Ollama;

/// <summary>
/// Queries the configured Ollama endpoint to determine local model availability.
/// </summary>
/// <param name="clientSettingsService">The workstation settings service.</param>
public sealed class OllamaService(IClientSettingsService clientSettingsService) : IOllamaService
{
    /// <summary>
    /// Gets the current Ollama connection status.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The current Ollama status snapshot.</returns>
    public async Task<OllamaConnectionStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var settings = await clientSettingsService.GetAsync(cancellationToken);
        var baseUrl = settings.OllamaBaseUrl;

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return new OllamaConnectionStatus(string.Empty, false, false, 0, "Ollama endpoint is not configured.", null);
        }

        try
        {
            using var httpClient = CreateHttpClient(baseUrl);
            using var response = await httpClient.GetAsync("api/tags", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new OllamaConnectionStatus(
                    baseUrl,
                    true,
                    false,
                    0,
                    $"Ollama responded with HTTP {(int)response.StatusCode}.",
                    response.ReasonPhrase);
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var modelCount = document.RootElement.TryGetProperty("models", out var models) && models.ValueKind == JsonValueKind.Array
                ? models.GetArrayLength()
                : 0;

            var summary = modelCount == 0
                ? "Ollama is reachable, but no local models were reported."
                : $"Ollama is reachable with {modelCount} local model(s).";

            return new OllamaConnectionStatus(baseUrl, true, true, modelCount, summary, null);
        }
        catch (Exception ex)
        {
            return new OllamaConnectionStatus(baseUrl, true, false, 0, "Unable to reach Ollama.", ex.Message);
        }
    }

    /// <summary>
    /// Creates a short-lived HTTP client for the configured Ollama endpoint.
    /// </summary>
    /// <param name="baseUrl">The configured Ollama base URL.</param>
    /// <returns>A configured HTTP client.</returns>
    private static HttpClient CreateHttpClient(string baseUrl)
    {
        return new HttpClient
        {
            BaseAddress = new Uri(EnsureTrailingSlash(baseUrl), UriKind.Absolute),
            Timeout = TimeSpan.FromSeconds(5)
        };
    }

    /// <summary>
    /// Ensures the supplied base URL ends with a trailing slash.
    /// </summary>
    /// <param name="value">The base URL to normalize.</param>
    /// <returns>The normalized URL.</returns>
    private static string EnsureTrailingSlash(string value)
    {
        return value.EndsWith("/", StringComparison.Ordinal) ? value : $"{value}/";
    }
}
