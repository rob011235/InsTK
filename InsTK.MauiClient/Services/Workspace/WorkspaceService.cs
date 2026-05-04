// Copyright (c) Robert Garner. All rights reserved.

using InsTK.MauiClient.Models;
using InsTK.MauiClient.Services.Settings;

namespace InsTK.MauiClient.Services.Workspace;

/// <summary>
/// Resolves and creates the local grading workspace folder structure.
/// </summary>
/// <param name="clientSettingsService">The workstation settings service.</param>
public sealed class WorkspaceService(IClientSettingsService clientSettingsService) : IWorkspaceService
{
    /// <summary>
    /// Gets the current workspace snapshot without creating directories.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The resolved workspace snapshot.</returns>
    public async Task<WorkspaceSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var settings = await clientSettingsService.GetAsync(cancellationToken);
        return CreateSnapshot(settings.WorkspaceRoot);
    }

    /// <summary>
    /// Ensures the local workspace directory structure exists.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The resolved workspace snapshot after creation.</returns>
    public async Task<WorkspaceSnapshot> EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = await GetSnapshotAsync(cancellationToken);

        Directory.CreateDirectory(snapshot.RootPath);
        Directory.CreateDirectory(snapshot.RepositoriesPath);
        Directory.CreateDirectory(snapshot.GradingRunsPath);
        Directory.CreateDirectory(snapshot.ReportsPath);

        return snapshot with { Exists = true };
    }

    /// <summary>
    /// Creates a workspace snapshot from the supplied root path.
    /// </summary>
    /// <param name="rootPath">The configured workspace root path.</param>
    /// <returns>The resolved workspace snapshot.</returns>
    private static WorkspaceSnapshot CreateSnapshot(string rootPath)
    {
        var fullRootPath = Path.GetFullPath(rootPath);

        return new WorkspaceSnapshot(
            fullRootPath,
            Path.Combine(fullRootPath, "Repositories"),
            Path.Combine(fullRootPath, "Runs"),
            Path.Combine(fullRootPath, "Reports"),
            Directory.Exists(fullRootPath));
    }
}
