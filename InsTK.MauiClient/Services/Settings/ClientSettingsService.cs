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
            WorkspaceRoot = GetDefaultWorkspaceRoot(),
            ManagedOllamaRoot = GetDefaultManagedOllamaRoot(),
            OllamaModelsRoot = GetDefaultOllamaModelsRoot()
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
            WorkspaceRoot = settings.WorkspaceRoot,
            RequiredOllamaVersion = settings.RequiredOllamaVersion,
            RequiredOllamaWindowsZipSha256 = settings.RequiredOllamaWindowsZipSha256,
            PrimaryOllamaModel = settings.PrimaryOllamaModel,
            FallbackOllamaModel = settings.FallbackOllamaModel,
            ManagedOllamaRoot = settings.ManagedOllamaRoot,
            OllamaModelsRoot = settings.OllamaModelsRoot
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
        settings.RequiredOllamaVersion = NormalizeRequiredValue(settings.RequiredOllamaVersion, "0.22.1");
        settings.RequiredOllamaWindowsZipSha256 = NormalizeRequiredValue(settings.RequiredOllamaWindowsZipSha256, "93c38a2ae97e4ab55c6d324e9cf62bc79408de85861045c34f4294c774d00c34").ToLowerInvariant();
        settings.PrimaryOllamaModel = NormalizeRequiredValue(settings.PrimaryOllamaModel, "qwen3-coder:30b");
        settings.FallbackOllamaModel = NormalizeRequiredValue(settings.FallbackOllamaModel, "deepseek-coder:6.7b");
        settings.ManagedOllamaRoot = NormalizeRequiredPath(settings.ManagedOllamaRoot, GetDefaultManagedOllamaRoot());
        settings.OllamaModelsRoot = NormalizeRequiredPath(settings.OllamaModelsRoot, GetDefaultOllamaModelsRoot());
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
    /// Trims a required non-path value and applies a fallback when it was not supplied.
    /// </summary>
    /// <param name="value">The value to normalize.</param>
    /// <param name="fallback">The fallback value.</param>
    /// <returns>The normalized value.</returns>
    private static string NormalizeRequiredValue(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    /// <summary>
    /// Normalizes a required local path, falling back when needed.
    /// </summary>
    /// <param name="path">The path to normalize.</param>
    /// <param name="fallback">The fallback path.</param>
    /// <returns>The normalized full path.</returns>
    private static string NormalizeRequiredPath(string? path, string fallback)
    {
        var candidate = string.IsNullOrWhiteSpace(path) ? fallback : path.Trim();
        return Path.GetFullPath(Environment.ExpandEnvironmentVariables(candidate));
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
    /// Gets the default local root for managed Ollama runtime files.
    /// </summary>
    /// <returns>The default managed runtime root path.</returns>
    private static string GetDefaultManagedOllamaRoot()
    {
        return Path.Combine(FileSystem.Current.AppDataDirectory, "Dependencies", "Ollama");
    }

    /// <summary>
    /// Gets the default local root for Ollama model storage.
    /// </summary>
    /// <returns>The default models root path.</returns>
    private static string GetDefaultOllamaModelsRoot()
    {
        return Path.Combine(GetDefaultWorkspaceRoot(), "Models");
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
