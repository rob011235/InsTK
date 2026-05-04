// Copyright (c) Robert Garner. All rights reserved.

namespace InsTK.MauiClient.Models;

/// <summary>
/// Represents a runtime activity update emitted during managed Ollama operations.
/// </summary>
/// <param name="Message">The activity message.</param>
/// <param name="IsError">A value indicating whether the message represents an error.</param>
/// <param name="CollapseKey">The optional key used to replace an earlier progress entry instead of appending a new row.</param>
public sealed record OllamaRuntimeActivityEventArgs(
    string Message,
    bool IsError,
    string? CollapseKey = null);
