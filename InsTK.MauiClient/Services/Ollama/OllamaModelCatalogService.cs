// Copyright (c) Robert Garner. All rights reserved.

using InsTK.MauiClient.Models;

namespace InsTK.MauiClient.Services.Ollama;

/// <summary>
/// Supplies curated Ollama model recommendations based on currently available workstation memory.
/// </summary>
public sealed class OllamaModelCatalogService : IOllamaModelCatalogService
{
    private const ulong OneGiB = 1024UL * 1024UL * 1024UL;

    private static readonly IReadOnlyList<OllamaModelOption> CuratedModels =
    [
        new("deepseek-coder:1.3b", "DeepSeek Coder 1.3B", "776 MB", 2UL * OneGiB, "Smallest coding profile for low-memory machines."),
        new("qwen2.5-coder:3b", "Qwen2.5 Coder 3B", "1.9 GB", 4UL * OneGiB, "Balanced lightweight coding model."),
        new("deepseek-coder:6.7b", "DeepSeek Coder 6.7B", "3.8 GB", 6UL * OneGiB, "Good default coding model for mid-range machines."),
        new("qwen2.5-coder:7b", "Qwen2.5 Coder 7B", "4.7 GB", 8UL * OneGiB, "Stronger coding model that still fits common workstations."),
        new("qwen2.5-coder:14b", "Qwen2.5 Coder 14B", "9.0 GB", 12UL * OneGiB, "Higher-capability coding model for larger-memory machines."),
        new("qwen3-coder:30b", "Qwen3 Coder 30B", "19 GB", 12UL * OneGiB, "Largest curated coding model; use only when enough memory is actually available.")
    ];

    /// <inheritdoc />
    public IReadOnlyList<OllamaModelOption> GetSupportedModels(WorkstationProfile profile)
    {
        var supported = CuratedModels
            .Where(model => profile.AvailableMemoryBytes >= model.MinimumRecommendedAvailableMemoryBytes)
            .ToArray();

        return supported.Length > 0 ? supported : [CuratedModels[0]];
    }

    /// <inheritdoc />
    public OllamaModelOption GetRecommendedPrimaryModel(WorkstationProfile profile)
    {
        return GetSupportedModels(profile).Last();
    }

    /// <inheritdoc />
    public OllamaModelOption GetRecommendedFallbackModel(WorkstationProfile profile)
    {
        var supported = GetSupportedModels(profile);
        return supported.Count > 1 ? supported[^2] : supported[0];
    }
}
