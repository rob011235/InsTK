// Copyright (c) Robert Garner. All rights reserved.

using InsTK.MauiClient.Models;

namespace InsTK.MauiClient.Services.Workspace;

/// <summary>
/// Defines operations for resolving and preparing the local grading workspace.
/// </summary>
public interface IWorkspaceService
{
    /// <summary>
    /// Gets the current workspace snapshot without creating directories.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The resolved workspace snapshot.</returns>
    Task<WorkspaceSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures the local workspace directory structure exists.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The resolved workspace snapshot after creation.</returns>
    Task<WorkspaceSnapshot> EnsureCreatedAsync(CancellationToken cancellationToken = default);
}
