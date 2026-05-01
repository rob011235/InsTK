// Copyright (c) Robert Garner. All rights reserved.

using InsTK.MauiClient.Models;

namespace InsTK.MauiClient.Services.Settings;

/// <summary>
/// Defines persistence operations for workstation configuration settings.
/// </summary>
public interface IClientSettingsService
{
    /// <summary>
    /// Loads the current workstation settings.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The current workstation settings.</returns>
    Task<DesktopClientSettings> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the supplied workstation settings.
    /// </summary>
    /// <param name="settings">The settings to persist.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    Task SaveAsync(DesktopClientSettings settings, CancellationToken cancellationToken = default);
}
