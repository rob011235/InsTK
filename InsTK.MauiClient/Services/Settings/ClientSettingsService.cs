// Copyright (c) Robert Garner. All rights reserved.

using System.Text.Json;
using InsTK.MauiClient.Models;

namespace InsTK.MauiClient.Services.Settings;

/// <summary>
/// Persists and normalizes workstation configuration settings on the local machine.
/// </summary>
public sealed class ClientSettingsService : IClientSettingsService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly SemaphoreSlim syncLock = new(1, 1);
    private DesktopClientSettings? cachedSettings;

    /// <summary>
    /// Loads the current workstation settings from the local settings file.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The current workstation settings.</returns>
    public async Task<DesktopClientSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        await syncLock.WaitAsync(cancellationToken);

        try
        {
            if (cachedSettings is not null)
            {
                return Clone(cachedSettings);
            }

            var settingsPath = GetSettingsPath();

            if (!File.Exists(settingsPath))
            {
                cachedSettings = CreateDefaultSettings();
                return Clone(cachedSettings);
            }

            await using var stream = File.OpenRead(settingsPath);
            cachedSettings = await JsonSerializer.DeserializeAsync<DesktopClientSettings>(stream, SerializerOptions, cancellationToken)
                ?? CreateDefaultSettings();

            Normalize(cachedSettings);

            return Clone(cachedSettings);
        }
        finally
        {
            syncLock.Release();
        }
    }

    /// <summary>
    /// Saves the supplied workstation settings to the local settings file.
    /// </summary>
    /// <param name="settings">The settings to persist.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    public async Task SaveAsync(DesktopClientSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        await syncLock.WaitAsync(cancellationToken);

        try
        {
            var normalized = Clone(settings);
            Normalize(normalized);

            var settingsPath = GetSettingsPath();
            Directory.CreateDirectory(Path.GetDirectoryName(settingsPath)!);

            await using var stream = File.Create(settingsPath);
            await JsonSerializer.SerializeAsync(stream, normalized, SerializerOptions, cancellationToken);

            cachedSettings = normalized;
        }
        finally
        {
            syncLock.Release();
        }
    }

    /// <summary>
    /// Creates the default workstation settings for first-run scenarios.
    /// </summary>
    /// <returns>The default workstation settings.</returns>
    private static DesktopClientSettings CreateDefaultSettings()
    {
        return new DesktopClientSettings
        {
            WorkspaceRoot = GetDefaultWorkspaceRoot()
        };
    }

    /// <summary>
    /// Creates a defensive copy of the supplied settings instance.
    /// </summary>
    /// <param name="settings">The settings to clone.</param>
    /// <returns>A copy of the supplied settings.</returns>
    private static DesktopClientSettings Clone(DesktopClientSettings settings)
    {
        return new DesktopClientSettings
        {
            BackendBaseUrl = settings.BackendBaseUrl,
            OllamaBaseUrl = settings.OllamaBaseUrl,
            WorkspaceRoot = settings.WorkspaceRoot
        };
    }

    /// <summary>
    /// Normalizes the supplied settings so persisted values are consistent.
    /// </summary>
    /// <param name="settings">The settings to normalize.</param>
    private static void Normalize(DesktopClientSettings settings)
    {
        settings.BackendBaseUrl = NormalizeOptionalUrl(settings.BackendBaseUrl);
        settings.OllamaBaseUrl = NormalizeRequiredUrl(settings.OllamaBaseUrl, "http://127.0.0.1:11434");
        settings.WorkspaceRoot = string.IsNullOrWhiteSpace(settings.WorkspaceRoot)
            ? GetDefaultWorkspaceRoot()
            : Path.GetFullPath(Environment.ExpandEnvironmentVariables(settings.WorkspaceRoot.Trim()));
    }

    /// <summary>
    /// Normalizes an optional URL value.
    /// </summary>
    /// <param name="url">The URL to normalize.</param>
    /// <returns>The normalized URL, or <see langword="null"/> when not provided.</returns>
    private static string? NormalizeOptionalUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        return NormalizeUrl(url);
    }

    /// <summary>
    /// Normalizes a required URL value, falling back when none was supplied.
    /// </summary>
    /// <param name="url">The URL to normalize.</param>
    /// <param name="fallback">The fallback URL to use.</param>
    /// <returns>The normalized URL.</returns>
    private static string NormalizeRequiredUrl(string? url, string fallback)
    {
        return string.IsNullOrWhiteSpace(url)
            ? fallback
            : NormalizeUrl(url);
    }

    /// <summary>
    /// Trims and de-duplicates URL formatting details.
    /// </summary>
    /// <param name="value">The URL value to normalize.</param>
    /// <returns>The normalized URL.</returns>
    private static string NormalizeUrl(string value)
    {
        return value.Trim().TrimEnd('/');
    }

    /// <summary>
    /// Gets the default local workspace root for the MAUI client.
    /// </summary>
    /// <returns>The default workspace path.</returns>
    private static string GetDefaultWorkspaceRoot()
    {
        return Path.Combine(FileSystem.Current.AppDataDirectory, "Workspace");
    }

    /// <summary>
    /// Gets the full path to the persisted settings file.
    /// </summary>
    /// <returns>The settings file path.</returns>
    private static string GetSettingsPath()
    {
        return Path.Combine(FileSystem.Current.AppDataDirectory, "settings", "client-settings.json");
    }
}
