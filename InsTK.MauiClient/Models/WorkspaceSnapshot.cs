// Copyright (c) Robert Garner. All rights reserved.

namespace InsTK.MauiClient.Models;

/// <summary>
/// Represents the resolved local workspace folder structure for the workstation.
/// </summary>
/// <param name="RootPath">The root workspace directory.</param>
/// <param name="RepositoriesPath">The directory used for cloned repositories.</param>
/// <param name="GradingRunsPath">The directory used for grading run artifacts.</param>
/// <param name="ReportsPath">The directory used for generated reports.</param>
/// <param name="Exists">A value indicating whether the workspace root already exists.</param>
public sealed record WorkspaceSnapshot(
    string RootPath,
    string RepositoriesPath,
    string GradingRunsPath,
    string ReportsPath,
    bool Exists);
