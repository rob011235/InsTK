// Copyright (c) Robert Garner. All rights reserved.

using System.Net.Http.Json;
using System.Text.Json.Serialization;
using InsTK.MauiClient.Services.Settings;

namespace InsTK.MauiClient.Services.Ollama;

/// <summary>
/// Uses the local Ollama HTTP API to generate responses from the configured workstation model.
/// </summary>
/// <param name="clientSettingsService">The workstation settings provider.</param>
public sealed class OllamaPromptService(IClientSettingsService clientSettingsService) : IOllamaPromptService
{
    private const string InsufficientMemoryMarker = "model requires more system memory";

    /// <inheritdoc />
    public async Task<OllamaPromptConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default)
    {
        var settings = await clientSettingsService.GetAsync(cancellationToken);
        return new OllamaPromptConfiguration(settings.OllamaBaseUrl, settings.PrimaryOllamaModel);
    }

    /// <inheritdoc />
    public async Task<OllamaPromptResult> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);

        var settings = await clientSettingsService.GetAsync(cancellationToken);
        var trimmedPrompt = prompt.Trim();

        using var httpClient = CreateHttpClient(settings.OllamaBaseUrl);
        var primaryAttempt = await GenerateWithModelAsync(httpClient, settings.PrimaryOllamaModel, trimmedPrompt, cancellationToken);

        if (primaryAttempt.IsSuccess)
        {
            return new OllamaPromptResult(primaryAttempt.Response!, settings.PrimaryOllamaModel, false);
        }

        var shouldTryFallback =
            IsInsufficientMemoryError(primaryAttempt.ErrorMessage)
            && !string.IsNullOrWhiteSpace(settings.FallbackOllamaModel)
            && !string.Equals(settings.PrimaryOllamaModel, settings.FallbackOllamaModel, StringComparison.OrdinalIgnoreCase);

        if (!shouldTryFallback)
        {
            throw new InvalidOperationException(primaryAttempt.ErrorMessage);
        }

        var fallbackAttempt = await GenerateWithModelAsync(httpClient, settings.FallbackOllamaModel, trimmedPrompt, cancellationToken);

        if (fallbackAttempt.IsSuccess)
        {
            return new OllamaPromptResult(fallbackAttempt.Response!, settings.FallbackOllamaModel, true);
        }

        throw new InvalidOperationException(
            $"Primary model '{settings.PrimaryOllamaModel}' exceeded available memory, and fallback model '{settings.FallbackOllamaModel}' also failed. {fallbackAttempt.ErrorMessage}");
    }

    private static HttpClient CreateHttpClient(string baseUrl)
    {
        return new HttpClient
        {
            BaseAddress = new Uri(EnsureTrailingSlash(baseUrl), UriKind.Absolute),
            Timeout = TimeSpan.FromMinutes(2)
        };
    }

    private static string EnsureTrailingSlash(string value)
    {
        return value.EndsWith("/", StringComparison.Ordinal) ? value : $"{value}/";
    }

    private static bool IsInsufficientMemoryError(string message)
    {
        return message.Contains(InsufficientMemoryMarker, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<OllamaGenerateAttempt> GenerateWithModelAsync(
        HttpClient httpClient,
        string model,
        string prompt,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/generate",
            new OllamaGenerateRequest(model, prompt, false),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var details = await response.Content.ReadAsStringAsync(cancellationToken);
            return OllamaGenerateAttempt.Fail(
                $"Ollama request failed with HTTP {(int)response.StatusCode} ({response.ReasonPhrase}) for model '{model}'. {details}".Trim());
        }

        var payload = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(cancellationToken: cancellationToken);

        if (payload is null || string.IsNullOrWhiteSpace(payload.Response))
        {
            return OllamaGenerateAttempt.Fail($"Ollama returned an empty response for model '{model}'.");
        }

        return OllamaGenerateAttempt.Succeed(payload.Response.Trim());
    }

    private sealed record OllamaGenerateRequest(string Model, string Prompt, bool Stream);

    private sealed record OllamaGenerateAttempt(bool IsSuccess, string? Response, string ErrorMessage)
    {
        public static OllamaGenerateAttempt Succeed(string response) => new(true, response, string.Empty);

        public static OllamaGenerateAttempt Fail(string errorMessage) => new(false, null, errorMessage);
    }

    private sealed class OllamaGenerateResponse
    {
        [JsonPropertyName("response")]
        public string? Response { get; init; }
    }
}
