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
            return new OllamaConnectionStatus(string.Empty, false, false, null, 0, Array.Empty<string>(), "Ollama endpoint is not configured.", null);
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
                    null,
                    0,
                    Array.Empty<string>(),
                    $"Ollama responded with HTTP {(int)response.StatusCode}.",
                    response.ReasonPhrase);
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var modelNames = GetModelNames(document.RootElement);
            var modelCount = modelNames.Count;
            var serverVersion = await TryGetServerVersionAsync(httpClient, cancellationToken);

            var summary = modelCount == 0
                ? "Ollama is reachable, but no local models were reported."
                : $"Ollama is reachable with {modelCount} local model(s).";

            return new OllamaConnectionStatus(baseUrl, true, true, serverVersion, modelCount, modelNames, summary, null);
        }
        catch (Exception ex)
        {
            return new OllamaConnectionStatus(baseUrl, true, false, null, 0, Array.Empty<string>(), "Unable to reach Ollama.", ex.Message);
        }
    }

    /// <summary>
    /// Reads the set of local model names reported by Ollama.
    /// </summary>
    /// <param name="root">The parsed API root element.</param>
    /// <returns>The reported model names.</returns>
    private static IReadOnlyList<string> GetModelNames(JsonElement root)
    {
        if (!root.TryGetProperty("models", out var models) || models.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<string>();
        }

        var modelNames = new List<string>();

        foreach (var model in models.EnumerateArray())
        {
            if (model.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String)
            {
                var value = name.GetString();

                if (!string.IsNullOrWhiteSpace(value))
                {
                    modelNames.Add(value);
                }
            }
        }

        return modelNames;
    }

    /// <summary>
    /// Attempts to read the Ollama server version from the local endpoint.
    /// </summary>
    /// <param name="httpClient">The configured HTTP client.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The reported server version, if available.</returns>
    private static async Task<string?> TryGetServerVersionAsync(HttpClient httpClient, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync("api/version", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            return document.RootElement.TryGetProperty("version", out var version) && version.ValueKind == JsonValueKind.String
                ? version.GetString()
                : null;
        }
        catch
        {
            return null;
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
