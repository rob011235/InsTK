// Copyright (c) Robert Garner. All rights reserved.

using InsTK.MauiClient.Models;

namespace InsTK.MauiClient.Services.Brightspace;

/// <summary>
/// Provides local Brightspace browser automation for login and submission scraping.
/// </summary>
public interface IBrightspaceAutomationService
{
    /// <summary>
    /// Raised whenever the service has a new activity line for the UI.
    /// </summary>
    event EventHandler<BrightspaceActivityEventArgs>? ActivityReported;

    /// <summary>
    /// Gets a value indicating whether a login browser session is currently open and awaiting save.
    /// </summary>
    bool IsLoginInProgress { get; }

    /// <summary>
    /// Opens a local browser session for manual Brightspace login.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    Task StartLoginAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the current Brightspace browser session state to the configured local session file.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    Task SaveLoginSessionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Scrapes the Brightspace Quick Eval queue and submission detail pages into a merged submission map.
    /// </summary>
    /// <param name="limit">An optional limit on processed submissions.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The merged submission-map result.</returns>
    Task<BrightspaceSubmissionMapResult> ScrapeSubmissionMapAsync(int? limit = null, CancellationToken cancellationToken = default);
}
