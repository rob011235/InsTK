// Copyright (c) Robert Garner. All rights reserved.

namespace InsTK.MauiClient.Models;

/// <summary>
/// Represents the result of a managed Ollama runtime operation.
/// </summary>
/// <param name="IsSuccess">A value indicating whether the operation completed successfully.</param>
/// <param name="Summary">A human-readable summary of the operation result.</param>
/// <param name="ErrorMessage">The detailed error message when the operation failed.</param>
public sealed record OllamaRuntimeOperationResult(
    bool IsSuccess,
    string Summary,
    string? ErrorMessage);
