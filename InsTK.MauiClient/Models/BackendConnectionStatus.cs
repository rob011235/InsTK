// Copyright (c) Robert Garner. All rights reserved.

namespace InsTK.MauiClient.Models;

/// <summary>
/// Represents the current backend connectivity state for the workstation.
/// </summary>
/// <param name="BaseUrl">The configured backend base URL.</param>
/// <param name="IsConfigured">A value indicating whether the backend URL has been configured.</param>
/// <param name="IsReachable">A value indicating whether the backend was reachable.</param>
/// <param name="AuthenticationState">A simple backend bootstrap or authentication state label.</param>
/// <param name="Summary">A human-readable summary of the current state.</param>
/// <param name="ErrorMessage">The last connection error, if any.</param>
public sealed record BackendConnectionStatus(
    string BaseUrl,
    bool IsConfigured,
    bool IsReachable,
    string AuthenticationState,
    string Summary,
    string? ErrorMessage);
