// Copyright (c) Robert Garner. All rights reserved.

using System.Text.RegularExpressions;

namespace InsTK.MauiClient.Models;

/// <summary>
/// Represents a discoverable Brightspace course option for the signed-in instructor.
/// </summary>
public sealed record BrightspaceCourseOption(
    string Name,
    string Url,
    string CourseId,
    string TermLabel,
    string? CourseCode);

/// <summary>
/// Represents the discovered Brightspace course catalog for the current session.
/// </summary>
public sealed record BrightspaceCourseCatalogResult(
    string BaseUrl,
    DateTimeOffset ScrapedAt,
    IReadOnlyList<string> TermLabels,
    IReadOnlyList<BrightspaceCourseOption> Courses);

/// <summary>
/// Represents a single Quick Eval row discovered in Brightspace.
/// </summary>
public sealed record BrightspaceQuickEvalSubmission(
    int Index,
    string? Student,
    string? ActivityName,
    string ActivityType,
    string AssignmentKey,
    string? SubmittedAt,
    string? EvaluationUrl,
    IReadOnlyList<string> Urls);

/// <summary>
/// Represents the detail extracted from one Brightspace evaluation page.
/// </summary>
public sealed record BrightspaceSubmissionDetail(
    string Scraper,
    DateTimeOffset ScrapedAt,
    string PageTitle,
    string PageUrl,
    string? PreviewUrl,
    string? RepoUrl,
    string? Owner,
    string? Repo,
    string? CloneUrl,
    string? BranchHint,
    string? SubdirHint,
    string? AssignmentPathHint,
    IReadOnlyList<string> Urls,
    string RawText);

/// <summary>
/// Represents one merged Quick Eval and evaluation-page entry.
/// </summary>
public sealed record BrightspaceSubmissionMapEntry(
    int Index,
    string? Student,
    string? ActivityName,
    string ActivityType,
    string AssignmentKey,
    string? SubmittedAt,
    string? EvaluationUrl,
    string? PageTitle,
    string? PreviewUrl,
    string? RepoUrl,
    string? Owner,
    string? Repo,
    string? CloneUrl,
    string? BranchHint,
    string? SubdirHint,
    string? AssignmentPathHint,
    IReadOnlyList<string> Urls,
    string RawText,
    string? Error);

/// <summary>
/// Represents a full Brightspace submission-map scrape.
/// </summary>
public sealed record BrightspaceSubmissionMapResult(
    string SchemaVersion,
    string Scraper,
    DateTimeOffset ScrapedAt,
    string PageUrl,
    int QuickEvalSubmissionCount,
    int ProcessedSubmissionCount,
    IReadOnlyList<BrightspaceSubmissionMapEntry> Submissions);

internal sealed record BrightspaceGitHubHints(
    string? Owner,
    string? Repo,
    string? CloneUrl,
    string? BranchHint,
    string? SubdirHint);

internal static class BrightspaceGitHubHintParser
{
    public static BrightspaceGitHubHints Parse(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return new BrightspaceGitHubHints(null, null, null, null, null);
        }

        if (!uri.Host.EndsWith("github.com", StringComparison.OrdinalIgnoreCase))
        {
            return new BrightspaceGitHubHints(null, null, null, null, null);
        }

        var parts = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var owner = parts.ElementAtOrDefault(0);
        var repo = parts.ElementAtOrDefault(1)?.Replace(".git", string.Empty, StringComparison.OrdinalIgnoreCase);
        string? branchHint = null;
        string? subdirHint = null;

        if (parts.Length > 3 && string.Equals(parts[2], "tree", StringComparison.OrdinalIgnoreCase))
        {
            branchHint = Uri.UnescapeDataString(parts[3]);
            subdirHint = parts.Length > 4 ? string.Join('/', parts[4..].Select(Uri.UnescapeDataString)) : null;
        }
        else if (parts.Length > 4 && string.Equals(parts[2], "blob", StringComparison.OrdinalIgnoreCase))
        {
            branchHint = Uri.UnescapeDataString(parts[3]);
            subdirHint = parts.Length > 5 ? string.Join('/', parts[4..^1].Select(Uri.UnescapeDataString)) : null;
        }

        return new BrightspaceGitHubHints(
            owner,
            repo,
            owner is not null && repo is not null ? $"https://github.com/{owner}/{repo}.git" : null,
            branchHint,
            subdirHint);
    }
}

internal static class BrightspaceAssignmentPathHintParser
{
    private static readonly Regex CheckPathRegex = new(
        @"(?:check|use|see|look\s+at)\s+([A-Za-z0-9._\-/]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string? Parse(string? rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return null;
        }

        var match = CheckPathRegex.Match(rawText);
        if (!match.Success)
        {
            return null;
        }

        var value = match.Groups[1].Value.Trim().TrimEnd('.', ',', ';', ':');
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}

internal static class BrightspaceAssignmentClassifier
{
    private static readonly Regex ProgramCodePrefixRegex = new(
        @"^\s*(?:P|E)\d+\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string GetActivityType(string? activityName)
        => ContainsProgramSignal(activityName) ? "program" : "tutorial";

    public static string GetAssignmentKey(string? activityName, string activityType)
    {
        if (string.IsNullOrWhiteSpace(activityName))
        {
            return $"{activityType}-unknown";
        }

        var normalized = activityName.ToLowerInvariant();
        normalized = Regex.Replace(normalized, @"[^a-z0-9]+", "-");
        normalized = normalized.Trim('-');
        return string.IsNullOrWhiteSpace(normalized) ? $"{activityType}-unknown" : $"{activityType}-{normalized}";
    }

    private static bool ContainsProgramSignal(string? activityName)
        => !string.IsNullOrWhiteSpace(activityName)
            && (activityName.Contains("Program", StringComparison.OrdinalIgnoreCase)
                || activityName.Contains("Competency", StringComparison.OrdinalIgnoreCase)
                || ProgramCodePrefixRegex.IsMatch(activityName));
}

internal static class BrightspaceTermLabelParser
{
    private static readonly Regex SeasonYearRegex = new(
        @"\b(Spring|Summer|Fall|Autumn|Winter)\s+20\d{2}\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex YearSeasonRegex = new(
        @"\b20\d{2}\s+(Spring|Summer|Fall|Autumn|Winter)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string ParseOrDefault(string? courseName)
    {
        if (!string.IsNullOrWhiteSpace(courseName))
        {
            var seasonYear = SeasonYearRegex.Match(courseName);
            if (seasonYear.Success)
            {
                return seasonYear.Value.Trim();
            }

            var yearSeason = YearSeasonRegex.Match(courseName);
            if (yearSeason.Success)
            {
                return yearSeason.Value.Trim();
            }
        }

        return "Uncategorized";
    }
}
