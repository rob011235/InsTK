// Copyright (c) Robert Garner. All rights reserved.

namespace InsTK.MauiClient.Models;

/// <summary>
/// Represents the high-level workstation readiness state for the managed Ollama runtime.
/// </summary>
public enum OllamaRuntimeState
{
    /// <summary>
    /// The managed runtime executable could not be found.
    /// </summary>
    Missing,

    /// <summary>
    /// The managed runtime was found, but its version does not match the required policy.
    /// </summary>
    WrongVersion,

    /// <summary>
    /// The managed runtime exists, but the local Ollama endpoint is not reachable.
    /// </summary>
    EndpointUnavailable,

    /// <summary>
    /// An Ollama endpoint is reachable, but it does not match the managed runtime policy.
    /// </summary>
    ConflictingInstance,

    /// <summary>
    /// The managed runtime is available, but the required models are not all installed.
    /// </summary>
    ModelMissing,

    /// <summary>
    /// The workstation satisfies the configured Ollama runtime and model policy.
    /// </summary>
    Ready
}
