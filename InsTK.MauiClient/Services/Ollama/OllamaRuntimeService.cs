// Copyright (c) Robert Garner. All rights reserved.

using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
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

    /// <inheritdoc />
    public event EventHandler<OllamaRuntimeActivityEventArgs>? ActivityReported;

    /// <summary>
    /// Evaluates the current workstation readiness for the configured managed Ollama runtime.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The evaluated runtime status.</returns>
    public async Task<OllamaRuntimeStatus> GetRuntimeStatusAsync(CancellationToken cancellationToken = default)
    {
        var settings = await clientSettingsService.GetAsync(cancellationToken);
        var executablePath = GetManagedExecutablePath(settings);
        var connectionStatus = await ollamaService.GetStatusAsync(cancellationToken);

        if (!File.Exists(executablePath))
        {
            if (connectionStatus.IsReachable)
            {
                return new OllamaRuntimeStatus(
                    OllamaRuntimeState.ConflictingInstance,
                    executablePath,
                    settings.RequiredOllamaVersion,
                    settings.RequiredOllamaWindowsZipSha256,
                    null,
                    connectionStatus.ServerVersion,
                    settings.PrimaryOllamaModel,
                    settings.FallbackOllamaModel,
                    false,
                    false,
                    true,
                    settings.OllamaModelsRoot,
                    "An Ollama endpoint is already reachable, but the managed runtime is not installed.",
                    null);
            }

            return new OllamaRuntimeStatus(
                OllamaRuntimeState.Missing,
                executablePath,
                settings.RequiredOllamaVersion,
                settings.RequiredOllamaWindowsZipSha256,
                null,
                null,
                settings.PrimaryOllamaModel,
                settings.FallbackOllamaModel,
                false,
                false,
                false,
                settings.OllamaModelsRoot,
                "Managed Ollama runtime is not installed on this workstation.",
                null);
        }

        var detectedVersion = await TryGetManagedVersionAsync(executablePath, cancellationToken);

        if (!string.Equals(detectedVersion, settings.RequiredOllamaVersion, StringComparison.OrdinalIgnoreCase))
        {
            var state = connectionStatus.IsReachable ? OllamaRuntimeState.ConflictingInstance : OllamaRuntimeState.WrongVersion;
            var summary = connectionStatus.IsReachable
                ? $"An Ollama endpoint is reachable, but the managed runtime version {DisplayVersion(detectedVersion)} does not match required version {settings.RequiredOllamaVersion}."
                : $"Managed Ollama version {DisplayVersion(detectedVersion)} does not match required version {settings.RequiredOllamaVersion}.";

            return new OllamaRuntimeStatus(
                state,
                executablePath,
                settings.RequiredOllamaVersion,
                settings.RequiredOllamaWindowsZipSha256,
                detectedVersion,
                connectionStatus.ServerVersion,
                settings.PrimaryOllamaModel,
                settings.FallbackOllamaModel,
                false,
                false,
                connectionStatus.IsReachable,
                settings.OllamaModelsRoot,
                summary,
                null);
        }

        if (!connectionStatus.IsReachable)
        {
            return new OllamaRuntimeStatus(
                OllamaRuntimeState.EndpointUnavailable,
                executablePath,
                settings.RequiredOllamaVersion,
                settings.RequiredOllamaWindowsZipSha256,
                detectedVersion,
                null,
                settings.PrimaryOllamaModel,
                settings.FallbackOllamaModel,
                false,
                false,
                false,
                settings.OllamaModelsRoot,
                "Managed Ollama runtime is installed, but the local endpoint is not reachable.",
                connectionStatus.ErrorMessage);
        }

        if (!string.IsNullOrWhiteSpace(connectionStatus.ServerVersion)
            && !string.Equals(connectionStatus.ServerVersion, settings.RequiredOllamaVersion, StringComparison.OrdinalIgnoreCase))
        {
            return new OllamaRuntimeStatus(
                OllamaRuntimeState.ConflictingInstance,
                executablePath,
                settings.RequiredOllamaVersion,
                settings.RequiredOllamaWindowsZipSha256,
                detectedVersion,
                connectionStatus.ServerVersion,
                settings.PrimaryOllamaModel,
                settings.FallbackOllamaModel,
                false,
                false,
                true,
                settings.OllamaModelsRoot,
                $"A running Ollama endpoint reports version {connectionStatus.ServerVersion}, which does not match required version {settings.RequiredOllamaVersion}.",
                null);
        }

        var hasPrimaryModel = connectionStatus.ModelNames.Contains(settings.PrimaryOllamaModel, StringComparer.OrdinalIgnoreCase);
        var hasFallbackModel = connectionStatus.ModelNames.Contains(settings.FallbackOllamaModel, StringComparer.OrdinalIgnoreCase);

        if (!hasPrimaryModel || !hasFallbackModel)
        {
            return new OllamaRuntimeStatus(
                OllamaRuntimeState.ModelMissing,
                executablePath,
                settings.RequiredOllamaVersion,
                settings.RequiredOllamaWindowsZipSha256,
                detectedVersion,
                connectionStatus.ServerVersion,
                settings.PrimaryOllamaModel,
                settings.FallbackOllamaModel,
                hasPrimaryModel,
                hasFallbackModel,
                false,
                settings.OllamaModelsRoot,
                BuildMissingModelSummary(hasPrimaryModel, hasFallbackModel, settings.PrimaryOllamaModel, settings.FallbackOllamaModel),
                null);
        }

        return new OllamaRuntimeStatus(
            OllamaRuntimeState.Ready,
            executablePath,
            settings.RequiredOllamaVersion,
            settings.RequiredOllamaWindowsZipSha256,
            detectedVersion,
            connectionStatus.ServerVersion,
            settings.PrimaryOllamaModel,
            settings.FallbackOllamaModel,
            true,
            true,
            false,
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
            ReportActivity($"Preparing managed Ollama runtime install for version {settings.RequiredOllamaVersion}.");
            Directory.CreateDirectory(downloadDirectory);
            Directory.CreateDirectory(settings.OllamaModelsRoot);

            var downloadUrl = BuildWindowsRuntimeDownloadUrl(settings.RequiredOllamaVersion);
            ReportActivity($"Downloading {OllamaWindowsAssetName} from the official Ollama release.");

            using (var httpClient = CreateDownloadHttpClient())
            using (var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            await using (var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken))
            await using (var outputStream = File.Create(zipPath))
            {
                response.EnsureSuccessStatusCode();
                await CopyWithProgressAsync(responseStream, outputStream, response.Content.Headers.ContentLength, cancellationToken);
            }

            ReportActivity("Verifying downloaded runtime checksum.");
            var calculatedSha256 = await ComputeSha256Async(zipPath, cancellationToken);

            if (!string.Equals(calculatedSha256, settings.RequiredOllamaWindowsZipSha256, StringComparison.OrdinalIgnoreCase))
            {
                return Fail("Managed Ollama runtime checksum validation failed.");
            }

            ReportActivity("Checksum validation succeeded.");
            TryDeleteDirectory(tempExtractDirectory);
            ReportActivity("Extracting managed runtime archive.");
            ZipFile.ExtractToDirectory(zipPath, tempExtractDirectory);

            TryDeleteDirectory(targetVersionDirectory);
            Directory.CreateDirectory(targetVersionDirectory);
            ReportActivity("Activating managed runtime files.");
            CopyExtractedRuntime(tempExtractDirectory, targetVersionDirectory);
            ReportActivity($"Managed Ollama runtime {settings.RequiredOllamaVersion} is installed.");

            return new OllamaRuntimeOperationResult(true, $"Managed Ollama runtime {settings.RequiredOllamaVersion} is installed.", null);
        }
        catch (Exception ex)
        {
            return Fail("Managed Ollama runtime installation failed.", ex.Message);
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
        var runtimeStatus = await GetRuntimeStatusAsync(cancellationToken);

        if (!File.Exists(executablePath))
        {
            return Fail("Managed Ollama runtime is not installed.");
        }

        if (runtimeStatus.ConflictDetected)
        {
            return Fail("Cannot start managed Ollama while a conflicting Ollama instance is already reachable.");
        }

        var existingStatus = await ollamaService.GetStatusAsync(cancellationToken);

        if (existingStatus.IsReachable)
        {
            return new OllamaRuntimeOperationResult(true, "Ollama is already running.", null);
        }

        try
        {
            Directory.CreateDirectory(settings.OllamaModelsRoot);
            ReportActivity("Starting managed Ollama runtime.");

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
                ReportActivity($"Waiting for managed Ollama endpoint to become reachable ({attempt + 1}/10).");

                if (status.IsReachable)
                {
                    ReportActivity("Managed Ollama runtime started successfully.");
                    return new OllamaRuntimeOperationResult(true, "Managed Ollama runtime started successfully.", null);
                }
            }

            return Fail("Managed Ollama runtime did not become reachable after startup.");
        }
        catch (Exception ex)
        {
            return Fail("Managed Ollama runtime startup failed.", ex.Message);
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
            ReportActivity($"Ensuring required model {model} is available locally.");
            var pullProgressKey = BuildModelPullCollapseKey(model);
            ReportActivity($"Downloading model {model}.", collapseKey: pullProgressKey);
            var pullResult = await RunManagedCommandAsync(
                executablePath,
                $"pull {model}",
                settings,
                cancellationToken,
                pullProgressCollapseKey: pullProgressKey);

            if (!pullResult.IsSuccess)
            {
                return Fail($"Failed while downloading required model {model}.", pullResult.ErrorMessage);
            }

            ReportActivity($"Model {model} is available locally.", collapseKey: pullProgressKey);
        }

        ReportActivity("Required Ollama grading models are installed.");
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
    private async Task<OllamaRuntimeOperationResult> RunManagedCommandAsync(
        string executablePath,
        string arguments,
        DesktopClientSettings settings,
        CancellationToken cancellationToken,
        string? pullProgressCollapseKey = null)
    {
        using var process = CreateManagedProcess(executablePath, arguments);
        process.StartInfo.EnvironmentVariables["OLLAMA_MODELS"] = settings.OllamaModelsRoot;
        process.StartInfo.EnvironmentVariables["OLLAMA_HOST"] = BuildOllamaHostValue(settings.OllamaBaseUrl);
        process.Start();

        var standardOutput = new List<string>();
        var standardError = new List<string>();
        var standardOutputTask = ReadProcessOutputAsync(
            process.StandardOutput,
            standardOutput,
            cancellationToken,
            collapseKey: pullProgressCollapseKey);
        var standardErrorTask = ReadProcessOutputAsync(
            process.StandardError,
            standardError,
            cancellationToken,
            isError: true,
            collapseKey: pullProgressCollapseKey);

        await Task.WhenAll(standardOutputTask, standardErrorTask, process.WaitForExitAsync(cancellationToken));

        if (process.ExitCode == 0)
        {
            var summary = standardOutput.LastOrDefault(static line => !string.IsNullOrWhiteSpace(line))
                ?? "Managed Ollama command completed successfully.";
            return new OllamaRuntimeOperationResult(true, summary, null);
        }

        var error = standardError.LastOrDefault(static line => !string.IsNullOrWhiteSpace(line))
            ?? standardOutput.LastOrDefault(static line => !string.IsNullOrWhiteSpace(line));
        return new OllamaRuntimeOperationResult(false, "Managed Ollama command failed.", error);
    }

    /// <summary>
    /// Copies a stream to a target file while reporting download progress.
    /// </summary>
    /// <param name="source">The source stream.</param>
    /// <param name="destination">The destination stream.</param>
    /// <param name="contentLength">The expected content length, if known.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    private async Task CopyWithProgressAsync(Stream source, Stream destination, long? contentLength, CancellationToken cancellationToken)
    {
        var buffer = new byte[1024 * 128];
        long totalBytesRead = 0;
        var lastReportedPercent = -1;

        while (true)
        {
            var bytesRead = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);

            if (bytesRead == 0)
            {
                break;
            }

            await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            totalBytesRead += bytesRead;

            if (contentLength is > 0)
            {
                var percent = (int)((totalBytesRead * 100L) / contentLength.Value);

                if (percent >= lastReportedPercent + 5 || percent == 100)
                {
                    lastReportedPercent = percent;
                    ReportActivity($"Download progress: {percent}%.");
                }
            }
        }
    }

    /// <summary>
    /// Computes the SHA-256 digest for a downloaded runtime archive.
    /// </summary>
    /// <param name="filePath">The file to hash.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The lowercase SHA-256 digest.</returns>
    private static async Task<string> ComputeSha256Async(string filePath, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(filePath);
        using var sha256 = SHA256.Create();
        var hash = await sha256.ComputeHashAsync(stream, cancellationToken);
        return Convert.ToHexStringLower(hash);
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
    /// Reads managed Ollama process output, reports each meaningful update, and captures the emitted lines.
    /// </summary>
    /// <param name="reader">The process output reader.</param>
    /// <param name="capturedLines">The list that receives emitted lines.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <param name="isError">A value indicating whether the stream represents error output.</param>
    private async Task ReadProcessOutputAsync(
        StreamReader reader,
        List<string> capturedLines,
        CancellationToken cancellationToken,
        bool isError = false,
        string? collapseKey = null)
    {
        var buffer = new char[256];
        var currentLine = new StringBuilder();
        string? lastReportedLine = null;

        while (true)
        {
            var charsRead = await reader.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);

            if (charsRead == 0)
            {
                break;
            }

            for (var index = 0; index < charsRead; index++)
            {
                var currentCharacter = buffer[index];

                if (currentCharacter == '\r' || currentCharacter == '\n')
                {
                    FlushOutputLine(currentLine, capturedLines, ref lastReportedLine, isError, collapseKey);
                    continue;
                }

                currentLine.Append(currentCharacter);
            }
        }

        FlushOutputLine(currentLine, capturedLines, ref lastReportedLine, isError, collapseKey);
    }

    /// <summary>
    /// Normalizes and reports a single managed Ollama CLI output line when it contains meaningful content.
    /// </summary>
    /// <param name="currentLine">The mutable line buffer.</param>
    /// <param name="capturedLines">The list that receives emitted lines.</param>
    /// <param name="lastReportedLine">The most recent line reported for this stream.</param>
    /// <param name="isError">A value indicating whether the line is from the error stream.</param>
    private void FlushOutputLine(
        StringBuilder currentLine,
        List<string> capturedLines,
        ref string? lastReportedLine,
        bool isError,
        string? collapseKey)
    {
        if (currentLine.Length == 0)
        {
            return;
        }

        var normalizedLine = NormalizeProcessOutput(currentLine.ToString());
        currentLine.Clear();

        if (string.IsNullOrWhiteSpace(normalizedLine))
        {
            return;
        }

        capturedLines.Add(normalizedLine);

        if (string.Equals(lastReportedLine, normalizedLine, StringComparison.Ordinal))
        {
            return;
        }

        lastReportedLine = normalizedLine;
        ReportActivity(normalizedLine, isError, collapseKey);
    }

    /// <summary>
    /// Builds a stable UI key for collapsing progress updates for a single model pull.
    /// </summary>
    /// <param name="model">The model being downloaded.</param>
    /// <returns>The collapse key.</returns>
    private static string BuildModelPullCollapseKey(string model)
    {
        return $"pull:{model.Trim()}";
    }

    /// <summary>
    /// Reduces raw CLI output to a compact single-line activity message.
    /// </summary>
    /// <param name="value">The raw line to normalize.</param>
    /// <returns>The normalized output line.</returns>
    private static string NormalizeProcessOutput(string value)
    {
        return string.Join(
            " ",
            value
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
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

    /// <summary>
    /// Emits a runtime activity update to subscribers.
    /// </summary>
    /// <param name="message">The activity message.</param>
    /// <param name="isError">A value indicating whether the message represents an error.</param>
    private void ReportActivity(string message, bool isError = false, string? collapseKey = null)
    {
        ActivityReported?.Invoke(this, new OllamaRuntimeActivityEventArgs(message, isError, collapseKey));
    }

    /// <summary>
    /// Builds a failed operation result while also reporting the error as activity.
    /// </summary>
    /// <param name="summary">The summary to expose.</param>
    /// <param name="errorMessage">The detailed error message, if any.</param>
    /// <returns>A failed operation result.</returns>
    private OllamaRuntimeOperationResult Fail(string summary, string? errorMessage = null)
    {
        ReportActivity(string.IsNullOrWhiteSpace(errorMessage) ? summary : $"{summary} {errorMessage}", true);
        return new OllamaRuntimeOperationResult(false, summary, errorMessage);
    }

    [GeneratedRegex(@"(?<version>\d+\.\d+\.\d+)")]
    private static partial Regex VersionPattern();
}
