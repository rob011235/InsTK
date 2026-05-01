// Copyright (c) Robert Garner. All rights reserved.

using InsTK.MauiClient.Models;

namespace InsTK.MauiClient.Services.Ollama;

/// <summary>
/// Provides the curated set of Ollama models supported by the workstation experience.
/// </summary>
public interface IOllamaModelCatalogService
{
    /// <summary>
    /// Gets the curated models that fit the supplied workstation profile.
    /// </summary>
    /// <param name="profile">The workstation profile used to filter supported models.</param>
    /// <returns>The supported models ordered from smaller to larger.</returns>
    IReadOnlyList<OllamaModelOption> GetSupportedModels(WorkstationProfile profile);

    /// <summary>
    /// Gets the best recommended primary model for the supplied workstation profile.
    /// </summary>
    /// <param name="profile">The workstation profile used to select a model.</param>
    /// <returns>The recommended primary model.</returns>
    OllamaModelOption GetRecommendedPrimaryModel(WorkstationProfile profile);

    /// <summary>
    /// Gets a reasonable fallback model for the supplied workstation profile.
    /// </summary>
    /// <param name="profile">The workstation profile used to select a fallback model.</param>
    /// <returns>The recommended fallback model.</returns>
    OllamaModelOption GetRecommendedFallbackModel(WorkstationProfile profile);
}
