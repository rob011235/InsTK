// Copyright (c) Robert Garner. All rights reserved.

using InsTK.MauiClient.Models;
using InsTK.MauiClient.Services.Settings;

namespace InsTK.MauiClient.Services.Backend;

/// <summary>
/// Probes the configured InsTK backend to determine whether workstation bootstrap can proceed.
/// </summary>
/// <param name="clientSettingsService">The workstation settings service.</param>
public sealed class BackendSessionService(IClientSettingsService clientSettingsService) : IBackendSessionService
{
    /// <summary>
    /// Gets the current backend connection status.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The current backend status snapshot.</returns>
    public async Task<BackendConnectionStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var settings = await clientSettingsService.GetAsync(cancellationToken);
        var baseUrl = settings.BackendBaseUrl;

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return new BackendConnectionStatus(string.Empty, false, false, "NotConfigured", "Backend URL is not configured.", null);
        }

        try
        {
            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri(EnsureTrailingSlash(baseUrl), UriKind.Absolute),
                Timeout = TimeSpan.FromSeconds(5)
            };

            using var request = new HttpRequestMessage(HttpMethod.Get, string.Empty);
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            var isReachable = (int)response.StatusCode < 500;
            var authState = response.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => "AuthRequired",
                System.Net.HttpStatusCode.Forbidden => "Forbidden",
                _ => "BootstrapReady"
            };

            var summary = response.StatusCode switch
            {
                System.Net.HttpStatusCode.OK => "Backend is reachable. Authentication wiring is the next step.",
                System.Net.HttpStatusCode.Unauthorized => "Backend is reachable and requires authentication.",
                System.Net.HttpStatusCode.Forbidden => "Backend is reachable but access is forbidden for the current request.",
                _ when isReachable => $"Backend is reachable with HTTP {(int)response.StatusCode}.",
                _ => $"Backend responded with HTTP {(int)response.StatusCode}."
            };

            return new BackendConnectionStatus(baseUrl, true, isReachable, authState, summary, null);
        }
        catch (Exception ex)
        {
            return new BackendConnectionStatus(baseUrl, true, false, "Unavailable", "Unable to reach the backend.", ex.Message);
        }
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
