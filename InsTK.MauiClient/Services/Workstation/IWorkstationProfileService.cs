// Copyright (c) Robert Garner. All rights reserved.

using InsTK.MauiClient.Models;

namespace InsTK.MauiClient.Services.Workstation;

/// <summary>
/// Provides hardware profile information for the current workstation.
/// </summary>
public interface IWorkstationProfileService
{
    /// <summary>
    /// Reads the current workstation profile.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The current workstation profile.</returns>
    Task<WorkstationProfile> GetProfileAsync(CancellationToken cancellationToken = default);
}
