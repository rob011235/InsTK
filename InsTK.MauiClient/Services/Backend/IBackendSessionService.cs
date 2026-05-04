// Copyright (c) Robert Garner. All rights reserved.

using InsTK.MauiClient.Models;

namespace InsTK.MauiClient.Services.Backend;

/// <summary>
/// Defines operations for checking the InsTK backend bootstrap state from the workstation.
/// </summary>
public interface IBackendSessionService
{
    /// <summary>
    /// Gets the current backend connection status.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The current backend status snapshot.</returns>
    Task<BackendConnectionStatus> GetStatusAsync(CancellationToken cancellationToken = default);
}
