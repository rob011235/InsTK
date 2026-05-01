// Copyright (c) Robert Garner. All rights reserved.

namespace InsTK.MauiClient.Models;

/// <summary>
/// Represents a Brightspace automation activity update for the MAUI UI.
/// </summary>
/// <param name="Message">The activity message.</param>
/// <param name="IsError">A value indicating whether the message represents an error.</param>
public sealed record BrightspaceActivityEventArgs(string Message, bool IsError = false);
