// Copyright (c) Robert Garner. All rights reserved.

using System.Diagnostics;
using System.IO.Compression;
using System.Text.RegularExpressions;
using InsTK.MauiClient.Models;
using InsTK.MauiClient.Services.Settings;

namespace InsTK.MauiClient.Services.Ollama;

/// <summary>
/// Evaluates workstation readiness for the managed Ollama runtime and required grading models.
/// </summary>
/// <param name="clientSettingsService">The workstation settings service.</param>
/// <param name="ollamaService">The raw Ollama connectivity probe service.</param>
public sealed partial class OllamaRuntimeService(
    IClientSettingsService clientSettingsService,
    IOllamaService ollamaService) : IOllamaRuntimeService
{
    private const string OllamaWindowsAssetName = "ollama-windows-amd64.zip";

    /// <summary>
    /// Evaluates the current workstation readiness for the configured managed Ollama runtime.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The evaluated runtime status.</returns>
    public async Task<OllamaRuntimeStatus> GetRuntimeStatusAsync(CancellationToken cancellationToken = default)
    {
        var settings = await clientSettingsService.GetAsync(cancellationToken);
        var executablePath = GetManagedExecutablePath(settings);

        if (!File.Exists(executablePath))
        {
            return new OllamaRuntimeStatus(
                OllamaRuntimeState.Missing,
                executablePath,
                settings.RequiredOllamaVersion,
                null,
                settings.PrimaryOllamaModel,
                settings.FallbackOllamaModel,
                false,
                false,
                settings.OllamaModelsRoot,
                "Managed Ollama runtime is not installed on this workstation.",
                null);
        }

        var detectedVersion = await TryGetManagedVersionAsync(executablePath, cancellationToken);

        if (!string.Equals(detectedVersion, settings.RequiredOllamaVersion, StringComparison.OrdinalIgnoreCase))
        {
            return new OllamaRuntimeStatus(
                OllamaRuntimeState.WrongVersion,
                executablePath,
                settings.RequiredOllamaVersion,
                detectedVersion,
                settings.PrimaryOllamaModel,
                settings.FallbackOllamaModel,
                false,
                false,
                settings.OllamaModelsRoot,
                $"Managed Ollama version {DisplayVersion(detectedVersion)} does not match required version {settings.RequiredOllamaVersion}.",
                null);
        }

        var connectionStatus = await ollamaService.GetStatusAsync(cancellationToken);

        if (!connectionStatus.IsReachable)
        {
            return new OllamaRuntimeStatus(
                OllamaRuntimeState.EndpointUnavailable,
                executablePath,
                settings.RequiredOllamaVersion,
                detectedVersion,
                settings.PrimaryOllamaModel,
                settings.FallbackOllamaModel,
                false,
                false,
                settings.OllamaModelsRoot,
                "Managed Ollama runtime is installed, but the local endpoint is not reachable.",
                connectionStatus.ErrorMessage);
        }

        var hasPrimaryModel = connectionStatus.ModelNames.Contains(settings.PrimaryOllamaModel, StringComparer.OrdinalIgnoreCase);
        var hasFallbackModel = connectionStatus.ModelNames.Contains(settings.FallbackOllamaModel, StringComparer.OrdinalIgnoreCase);

        if (!hasPrimaryModel || !hasFallbackModel)
        {
            return new OllamaRuntimeStatus(
                OllamaRuntimeState.ModelMissing,
                executablePath,
                settings.RequiredOllamaVersion,
                detectedVersion,
                settings.PrimaryOllamaModel,
                settings.FallbackOllamaModel,
                hasPrimaryModel,
                hasFallbackModel,
                settings.OllamaModelsRoot,
                BuildMissingModelSummary(hasPrimaryModel, hasFallbackModel, settings.PrimaryOllamaModel, settings.FallbackOllamaModel),
                null);
        }

        return new OllamaRuntimeStatus(
            OllamaRuntimeState.Ready,
            executablePath,
            settings.RequiredOllamaVersion,
            detectedVersion,
            settings.PrimaryOllamaModel,
            settings.FallbackOllamaModel,
            true,
            true,
            settings.OllamaModelsRoot,
            "Managed Ollama runtime and required grading models are ready.",
            null);
    }

    /// <summary>
    /// Downloads and installs or updates the managed Ollama runtime required by workstation policy.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The runtime installation result.</returns>
    public async Task<OllamaRuntimeOperationResult> InstallOrUpdateManagedRuntimeAsync(CancellationToken cancellationToken = default)
    {
        var settings = await clientSettingsService.GetAsync(cancellationToken);
        var targetVersionDirectory = Path.Combine(settings.ManagedOllamaRoot, settings.RequiredOllamaVersion);
        var downloadDirectory = Path.Combine(settings.ManagedOllamaRoot, "_downloads");
        var tempExtractDirectory = Path.Combine(downloadDirectory, $"extract-{Guid.NewGuid():N}");
        var zipPath = Path.Combine(downloadDirectory, $"{settings.RequiredOllamaVersion}-{OllamaWindowsAssetName}");

        try
        {
            Directory.CreateDirectory(downloadDirectory);
            Directory.CreateDirectory(settings.OllamaModelsRoot);

            var downloadUrl = BuildWindowsRuntimeDownloadUrl(settings.RequiredOllamaVersion);

            using (var httpClient = CreateDownloadHttpClient())
            await using (var responseStream = await httpClient.GetStreamAsync(downloadUrl, cancellationToken))
            await using (var outputStream = File.Create(zipPath))
            {
                await responseStream.CopyToAsync(outputStream, cancellationToken);
            }

            TryDeleteDirectory(tempExtractDirectory);
            ZipFile.ExtractToDirectory(zipPath, tempExtractDirectory);

            TryDeleteDirectory(targetVersionDirectory);
            Directory.CreateDirectory(targetVersionDirectory);
            CopyExtractedRuntime(tempExtractDirectory, targetVersionDirectory);

            return new OllamaRuntimeOperationResult(true, $"Managed Ollama runtime {settings.RequiredOllamaVersion} is installed.", null);
        }
        catch (Exception ex)
        {
            return new OllamaRuntimeOperationResult(false, "Managed Ollama runtime installation failed.", ex.Message);
        }
        finally
        {
            TryDeleteFile(zipPath);
            TryDeleteDirectory(tempExtractDirectory);
        }
    }

    /// <summary>
    /// Starts the managed Ollama runtime using the configured local model storage path.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The runtime start result.</returns>
    public async Task<OllamaRuntimeOperationResult> StartManagedRuntimeAsync(CancellationToken cancellationToken = default)
    {
        var settings = await clientSettingsService.GetAsync(cancellationToken);
        var executablePath = GetManagedExecutablePath(settings);

        if (!File.Exists(executablePath))
        {
            return new OllamaRuntimeOperationResult(false, "Managed Ollama runtime is not installed.", null);
        }

        var existingStatus = await ollamaService.GetStatusAsync(cancellationToken);

        if (existingStatus.IsReachable)
        {
            return new OllamaRuntimeOperationResult(true, "Ollama is already running.", null);
        }

        try
        {
            Directory.CreateDirectory(settings.OllamaModelsRoot);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = "serve",
                    WorkingDirectory = Path.GetDirectoryName(executablePath)!,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.StartInfo.EnvironmentVariables["OLLAMA_MODELS"] = settings.OllamaModelsRoot;
            process.StartInfo.EnvironmentVariables["OLLAMA_HOST"] = BuildOllamaHostValue(settings.OllamaBaseUrl);
            process.Start();

            for (var attempt = 0; attempt < 10; attempt++)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                var status = await ollamaService.GetStatusAsync(cancellationToken);

                if (status.IsReachable)
                {
                    return new OllamaRuntimeOperationResult(true, "Managed Ollama runtime started successfully.", null);
                }
            }

            return new OllamaRuntimeOperationResult(false, "Managed Ollama runtime did not become reachable after startup.", null);
        }
        catch (Exception ex)
        {
            return new OllamaRuntimeOperationResult(false, "Managed Ollama runtime startup failed.", ex.Message);
        }
    }

    /// <summary>
    /// Downloads the required grading models defined by workstation policy.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The model download result.</returns>
    public async Task<OllamaRuntimeOperationResult> EnsureRequiredModelsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await clientSettingsService.GetAsync(cancellationToken);
        var executablePath = GetManagedExecutablePath(settings);

        if (!File.Exists(executablePath))
        {
            return new OllamaRuntimeOperationResult(false, "Managed Ollama runtime must be installed before models can be downloaded.", null);
        }

        var startResult = await StartManagedRuntimeAsync(cancellationToken);

        if (!startResult.IsSuccess)
        {
            return startResult;
        }

        var modelsToEnsure = new[] { settings.PrimaryOllamaModel, settings.FallbackOllamaModel }
            .Where(static model => !string.IsNullOrWhiteSpace(model))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var model in modelsToEnsure)
        {
            var pullResult = await RunManagedCommandAsync(executablePath, $"pull {model}", settings, cancellationToken);

            if (!pullResult.IsSuccess)
            {
                return new OllamaRuntimeOperationResult(false, $"Failed while downloading required model {model}.", pullResult.ErrorMessage);
            }
        }

        return new OllamaRuntimeOperationResult(true, "Required Ollama grading models are installed.", null);
    }

    /// <summary>
    /// Gets the expected path to the managed Ollama executable.
    /// </summary>
    /// <param name="settings">The persisted workstation settings.</param>
    /// <returns>The full executable path.</returns>
    private static string GetManagedExecutablePath(DesktopClientSettings settings)
    {
        return Path.Combine(settings.ManagedOllamaRoot, settings.RequiredOllamaVersion, "ollama.exe");
    }

    /// <summary>
    /// Builds the official GitHub release download URL for the Windows standalone Ollama zip.
    /// </summary>
    /// <param name="version">The Ollama version to download.</param>
    /// <returns>The download URL.</returns>
    private static string BuildWindowsRuntimeDownloadUrl(string version)
    {
        return $"https://github.com/ollama/ollama/releases/download/v{version}/{OllamaWindowsAssetName}";
    }

    /// <summary>
    /// Creates an HTTP client suitable for downloading large runtime packages.
    /// </summary>
    /// <returns>A configured HTTP client.</returns>
    private static HttpClient CreateDownloadHttpClient()
    {
        return new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(30)
        };
    }

    /// <summary>
    /// Attempts to read the version reported by the managed Ollama executable.
    /// </summary>
    /// <param name="executablePath">The full path to the managed executable.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The normalized version string, if it could be read.</returns>
    private static async Task<string?> TryGetManagedVersionAsync(string executablePath, CancellationToken cancellationToken)
    {
        using var process = CreateManagedProcess(executablePath, "-v");
        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        return TryExtractVersion(output);
    }

    /// <summary>
    /// Runs a managed Ollama CLI command and returns the result.
    /// </summary>
    /// <param name="executablePath">The full path to the managed executable.</param>
    /// <param name="arguments">The command arguments to run.</param>
    /// <param name="settings">The persisted workstation settings.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The operation result.</returns>
    private static async Task<OllamaRuntimeOperationResult> RunManagedCommandAsync(
        string executablePath,
        string arguments,
        DesktopClientSettings settings,
        CancellationToken cancellationToken)
    {
        using var process = CreateManagedProcess(executablePath, arguments);
        process.StartInfo.EnvironmentVariables["OLLAMA_MODELS"] = settings.OllamaModelsRoot;
        process.StartInfo.EnvironmentVariables["OLLAMA_HOST"] = BuildOllamaHostValue(settings.OllamaBaseUrl);
        process.Start();

        var standardOutput = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var standardError = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode == 0)
        {
            var summary = string.IsNullOrWhiteSpace(standardOutput) ? "Managed Ollama command completed successfully." : standardOutput.Trim();
            return new OllamaRuntimeOperationResult(true, summary, null);
        }

        var error = string.IsNullOrWhiteSpace(standardError) ? standardOutput : standardError;
        return new OllamaRuntimeOperationResult(false, "Managed Ollama command failed.", string.IsNullOrWhiteSpace(error) ? null : error.Trim());
    }

    /// <summary>
    /// Creates a managed Ollama process with redirected output.
    /// </summary>
    /// <param name="executablePath">The full path to the managed executable.</param>
    /// <param name="arguments">The CLI arguments to apply.</param>
    /// <returns>A configured process instance.</returns>
    private static Process CreateManagedProcess(string executablePath, string arguments)
    {
        return new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = arguments,
                WorkingDirectory = Path.GetDirectoryName(executablePath)!,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
    }

    /// <summary>
    /// Extracts a semantic-looking version number from Ollama CLI output.
    /// </summary>
    /// <param name="output">The CLI output to parse.</param>
    /// <returns>The parsed version, if one was found.</returns>
    private static string? TryExtractVersion(string? output)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            return null;
        }

        var match = VersionPattern().Match(output);
        return match.Success ? match.Groups["version"].Value : null;
    }

    /// <summary>
    /// Builds a user-facing summary for missing required grading models.
    /// </summary>
    /// <param name="hasPrimaryModel">A value indicating whether the primary model exists.</param>
    /// <param name="hasFallbackModel">A value indicating whether the fallback model exists.</param>
    /// <param name="primaryModel">The configured primary model.</param>
    /// <param name="fallbackModel">The configured fallback model.</param>
    /// <returns>A human-readable summary of the missing model state.</returns>
    private static string BuildMissingModelSummary(bool hasPrimaryModel, bool hasFallbackModel, string primaryModel, string fallbackModel)
    {
        return (hasPrimaryModel, hasFallbackModel) switch
        {
            (false, false) => $"Required grading models are missing: {primaryModel} and {fallbackModel}.",
            (false, true) => $"Primary grading model is missing: {primaryModel}.",
            (true, false) => $"Fallback grading model is missing: {fallbackModel}.",
            _ => "Required grading models are missing."
        };
    }

    /// <summary>
    /// Returns a fallback label for a missing version string.
    /// </summary>
    /// <param name="version">The detected version value.</param>
    /// <returns>A version string suitable for display.</returns>
    private static string DisplayVersion(string? version)
    {
        return string.IsNullOrWhiteSpace(version) ? "unknown" : version;
    }

    /// <summary>
    /// Copies the extracted runtime contents into the target version directory.
    /// </summary>
    /// <param name="extractRoot">The temporary extraction root.</param>
    /// <param name="targetDirectory">The final version directory.</param>
    private static void CopyExtractedRuntime(string extractRoot, string targetDirectory)
    {
        var sourceDirectory = ResolveExtractedRuntimeRoot(extractRoot);

        foreach (var directory in Directory.GetDirectories(sourceDirectory))
        {
            var destination = Path.Combine(targetDirectory, Path.GetFileName(directory));
            DirectoryCopy(directory, destination);
        }

        foreach (var file in Directory.GetFiles(sourceDirectory))
        {
            var destination = Path.Combine(targetDirectory, Path.GetFileName(file));
            File.Copy(file, destination, overwrite: true);
        }
    }

    /// <summary>
    /// Resolves the directory within the extracted archive that contains <c>ollama.exe</c>.
    /// </summary>
    /// <param name="extractRoot">The temporary extraction root.</param>
    /// <returns>The directory containing the runtime files.</returns>
    private static string ResolveExtractedRuntimeRoot(string extractRoot)
    {
        var executablePath = Directory
            .EnumerateFiles(extractRoot, "ollama.exe", SearchOption.AllDirectories)
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(executablePath))
        {
            throw new InvalidOperationException("Downloaded Ollama archive did not contain ollama.exe.");
        }

        return Path.GetDirectoryName(executablePath)!;
    }

    /// <summary>
    /// Recursively copies a directory to a destination path.
    /// </summary>
    /// <param name="sourceDirectory">The source directory.</param>
    /// <param name="destinationDirectory">The destination directory.</param>
    private static void DirectoryCopy(string sourceDirectory, string destinationDirectory)
    {
        Directory.CreateDirectory(destinationDirectory);

        foreach (var file in Directory.GetFiles(sourceDirectory))
        {
            var destination = Path.Combine(destinationDirectory, Path.GetFileName(file));
            File.Copy(file, destination, overwrite: true);
        }

        foreach (var directory in Directory.GetDirectories(sourceDirectory))
        {
            var destination = Path.Combine(destinationDirectory, Path.GetFileName(directory));
            DirectoryCopy(directory, destination);
        }
    }

    /// <summary>
    /// Builds the <c>OLLAMA_HOST</c> environment variable from the configured base URL.
    /// </summary>
    /// <param name="baseUrl">The configured Ollama base URL.</param>
    /// <returns>The host and port value expected by Ollama.</returns>
    private static string BuildOllamaHostValue(string baseUrl)
    {
        if (!Uri.TryCreate(EnsureTrailingSlash(baseUrl), UriKind.Absolute, out var uri))
        {
            return "127.0.0.1:11434";
        }

        return $"{uri.Host}:{uri.Port}";
    }

    /// <summary>
    /// Ensures the supplied base URL ends with a trailing slash.
    /// </summary>
    /// <param name="value">The base URL to normalize.</param>
    /// <returns>The normalized URL.</returns>
    private static string EnsureTrailingSlash(string value)
    {
        return value.EndsWith("/", StringComparison.Ordinal) ? value : $"{value}/";
    }

    /// <summary>
    /// Attempts to delete a temporary directory without surfacing cleanup errors.
    /// </summary>
    /// <param name="path">The directory path to delete.</param>
    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch
        {
        }
    }

    /// <summary>
    /// Attempts to delete a temporary file without surfacing cleanup errors.
    /// </summary>
    /// <param name="path">The file path to delete.</param>
    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
        }
    }

    [GeneratedRegex(@"(?<version>\d+\.\d+\.\d+)")]
    private static partial Regex VersionPattern();
}
