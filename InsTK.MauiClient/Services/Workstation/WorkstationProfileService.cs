// Copyright (c) Robert Garner. All rights reserved.

using InsTK.MauiClient.Models;
using System.Runtime.InteropServices;

namespace InsTK.MauiClient.Services.Workstation;

/// <summary>
/// Reads local machine hardware information used to guide Ollama model recommendations.
/// </summary>
public sealed class WorkstationProfileService : IWorkstationProfileService
{
    private const ulong OneGiB = 1024UL * 1024UL * 1024UL;
    private const string SmallModel = "deepseek-coder:1.3b";
    private const string MediumModel = "deepseek-coder:6.7b";
    private const string LargerModel = "qwen2.5-coder:7b";
    private const string LargestModel = "qwen3-coder:30b";

    /// <inheritdoc />
    public Task<WorkstationProfile> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var memoryStatus = GetMemoryStatus();
        var totalMemoryBytes = memoryStatus.ullTotalPhys;
        var availableMemoryBytes = memoryStatus.ullAvailPhys;
        var recommendedModel = RecommendPrimaryModel(availableMemoryBytes);
        var recommendationSummary = BuildRecommendationSummary(totalMemoryBytes, availableMemoryBytes, recommendedModel);

        return Task.FromResult(new WorkstationProfile(
            OperatingSystem: System.Runtime.InteropServices.RuntimeInformation.OSDescription,
            ProcessorCount: Environment.ProcessorCount,
            TotalMemoryBytes: totalMemoryBytes,
            AvailableMemoryBytes: availableMemoryBytes,
            RecommendedPrimaryModel: recommendedModel,
            RecommendationSummary: recommendationSummary));
    }

    private static string RecommendPrimaryModel(ulong availableMemoryBytes)
    {
        if (availableMemoryBytes >= 12UL * OneGiB)
        {
            return LargestModel;
        }

        if (availableMemoryBytes >= 8UL * OneGiB)
        {
            return LargerModel;
        }

        if (availableMemoryBytes >= 6UL * OneGiB)
        {
            return MediumModel;
        }

        return SmallModel;
    }

    private static string BuildRecommendationSummary(ulong totalMemoryBytes, ulong availableMemoryBytes, string recommendedModel)
    {
        var totalGiB = FormatGiB(totalMemoryBytes);
        var availableGiB = FormatGiB(availableMemoryBytes);

        return recommendedModel switch
        {
            LargestModel => $"This machine reports {totalGiB} GiB total RAM with about {availableGiB} GiB currently available. The larger {LargestModel} profile is reasonable.",
            LargerModel => $"This machine reports {totalGiB} GiB total RAM with about {availableGiB} GiB currently available. {LargerModel} is a safer primary model than {LargestModel}.",
            MediumModel => $"This machine reports {totalGiB} GiB total RAM with about {availableGiB} GiB currently available. Use {MediumModel} as the primary model on this machine.",
            _ => $"This machine reports {totalGiB} GiB total RAM with about {availableGiB} GiB currently available. Stay with a small model such as {SmallModel}."
        };
    }

    private static string FormatGiB(ulong bytes)
    {
        return (bytes / (double)OneGiB).ToString("0.0");
    }

    private static MemoryStatusEx GetMemoryStatus()
    {
        var memoryStatus = new MemoryStatusEx();

        if (!GlobalMemoryStatusEx(memoryStatus))
        {
            throw new InvalidOperationException("Unable to read physical memory information from the current machine.");
        }

        return memoryStatus;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MemoryStatusEx buffer);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private sealed class MemoryStatusEx
    {
        public MemoryStatusEx()
        {
            dwLength = (uint)Marshal.SizeOf<MemoryStatusEx>();
        }

        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }
}
