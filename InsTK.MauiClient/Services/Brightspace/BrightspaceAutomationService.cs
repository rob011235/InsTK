// Copyright (c) Robert Garner. All rights reserved.

using System.Text.Json;
using System.Text.RegularExpressions;
using InsTK.MauiClient.Models;
using InsTK.MauiClient.Services.Settings;
using Microsoft.Playwright;
using PlaywrightBrowser = Microsoft.Playwright.IBrowser;

namespace InsTK.MauiClient.Services.Brightspace;

/// <summary>
/// Runs local Playwright-based Brightspace automation on the instructor workstation.
/// </summary>
public sealed class BrightspaceAutomationService(IClientSettingsService clientSettingsService) : IBrightspaceAutomationService, IAsyncDisposable
{
    private const string ArtifactSchemaVersion = "1.0";
    private const string ScraperName = "InsTK";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private readonly SemaphoreSlim gate = new(1, 1);
    private IPlaywright? playwright;
    private PlaywrightBrowser? browser;
    private IBrowserContext? loginContext;

    /// <inheritdoc />
    public event EventHandler<BrightspaceActivityEventArgs>? ActivityReported;

    /// <inheritdoc />
    public bool IsLoginInProgress => loginContext is not null;

    /// <inheritdoc />
    public async Task StartLoginAsync(CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);

        try
        {
            if (loginContext is not null)
            {
                throw new InvalidOperationException("A Brightspace login session is already open. Save the session before starting another login.");
            }

            var settings = await clientSettingsService.GetAsync(cancellationToken);
            var baseUrl = settings.BrightspaceBaseUrl
                ?? throw new InvalidOperationException("Brightspace base URL is not configured.");
            var statePath = settings.BrightspaceStatePath;

            Directory.CreateDirectory(Path.GetDirectoryName(statePath)!);

            var driverPath = PlaywrightDriverLocator.EnsureConfigured();
            ReportActivity(PlaywrightDriverLocator.Describe(driverPath));

            playwright = await Playwright.CreateAsync();
            browser = await LaunchBrowserAsync(playwright, settings.BrightspaceBrowserChannel, headless: false);
            loginContext = await browser.NewContextAsync();
            var page = await loginContext.NewPageAsync();

            ReportActivity($"Opening {baseUrl}");
            await page.GotoAsync(baseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            ReportActivity("Complete the Brightspace login in the browser, then use 'Save Session' in the app.");
        }
        catch (Exception ex)
        {
            ReportActivity(ex.Message, true);
            await DisposeLoginResourcesAsync();
            throw;
        }
        finally
        {
            gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task SaveLoginSessionAsync(CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);

        try
        {
            if (loginContext is null)
            {
                throw new InvalidOperationException("No Brightspace login session is currently open.");
            }

            var settings = await clientSettingsService.GetAsync(cancellationToken);
            var statePath = settings.BrightspaceStatePath;
            Directory.CreateDirectory(Path.GetDirectoryName(statePath)!);

            await loginContext.StorageStateAsync(new BrowserContextStorageStateOptions { Path = statePath });
            ReportActivity($"Saved Brightspace session state to {statePath}");
        }
        finally
        {
            await DisposeLoginResourcesAsync();
            gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<BrightspaceCourseCatalogResult> DiscoverCoursesAsync(CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);

        try
        {
            if (loginContext is not null)
            {
                throw new InvalidOperationException("Save the open Brightspace login session before discovering courses.");
            }

            var settings = await clientSettingsService.GetAsync(cancellationToken);
            var baseUrl = settings.BrightspaceBaseUrl
                ?? throw new InvalidOperationException("Brightspace base URL is not configured.");
            var statePath = settings.BrightspaceStatePath;

            if (!File.Exists(statePath))
            {
                throw new InvalidOperationException("Brightspace session state was not found. Run Brightspace login first.");
            }

            var driverPath = PlaywrightDriverLocator.EnsureConfigured();
            ReportActivity(PlaywrightDriverLocator.Describe(driverPath));

            using var ephemeralPlaywright = await Playwright.CreateAsync();
            await using var ephemeralBrowser = await LaunchBrowserAsync(ephemeralPlaywright, settings.BrightspaceBrowserChannel, headless: true);
            var context = await ephemeralBrowser.NewContextAsync(new BrowserNewContextOptions
            {
                StorageStatePath = statePath,
            });

            var page = await context.NewPageAsync();
            ReportActivity($"Discovering courses from {baseUrl}");
            await page.GotoAsync(baseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var courses = await ReadCourseCatalogAcrossTermsAsync(page, baseUrl);
            if (courses.Count == 0)
            {
                throw await BuildCourseDiscoveryExceptionAsync(page, "No Brightspace course links were detected on the authenticated landing page.");
            }

            var termLabels = courses
                .Select(static course => course.TermLabel)
                .Where(static label => !string.IsNullOrWhiteSpace(label))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(static label => label, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            ReportActivity($"Discovered {courses.Count} course(s) across {termLabels.Length} term group(s).");
            return new BrightspaceCourseCatalogResult(
                baseUrl,
                DateTimeOffset.UtcNow,
                termLabels,
                courses);
        }
        catch (Exception ex)
        {
            ReportActivity(ex.Message, true);
            throw;
        }
        finally
        {
            gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<BrightspaceSubmissionMapResult> ScrapeSubmissionMapAsync(int? limit = null, CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);

        try
        {
            if (loginContext is not null)
            {
                throw new InvalidOperationException("Save the open Brightspace login session before starting a scrape.");
            }

            var settings = await clientSettingsService.GetAsync(cancellationToken);
            var quickEvalUrl = settings.BrightspaceBaseUrl
                ?? throw new InvalidOperationException("Brightspace base URL is not configured.");
            var statePath = settings.BrightspaceStatePath;
            var outPath = settings.BrightspaceSubmissionMapOutPath;

            if (!File.Exists(statePath))
            {
                throw new InvalidOperationException("Brightspace session state was not found. Run Brightspace login first.");
            }

            var driverPath = PlaywrightDriverLocator.EnsureConfigured();
            ReportActivity(PlaywrightDriverLocator.Describe(driverPath));

            using var ephemeralPlaywright = await Playwright.CreateAsync();
            await using var ephemeralBrowser = await LaunchBrowserAsync(ephemeralPlaywright, settings.BrightspaceBrowserChannel, headless: true);
            var context = await ephemeralBrowser.NewContextAsync(new BrowserNewContextOptions
            {
                StorageStatePath = statePath,
            });

            var quickEvalPage = await context.NewPageAsync();
            var quickEvalSubmissions = await ScrapeQuickEvalSubmissionsAsync(quickEvalPage, quickEvalUrl, scrapeAllPages: true, cancellationToken);
            var targetSubmissions = limit.HasValue
                ? quickEvalSubmissions.Take(limit.Value).ToList()
                : quickEvalSubmissions;

            var entries = new List<BrightspaceSubmissionMapEntry>();

            for (var i = 0; i < targetSubmissions.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var submission = targetSubmissions[i];

                if (string.IsNullOrWhiteSpace(submission.EvaluationUrl))
                {
                    entries.Add(new BrightspaceSubmissionMapEntry(
                        submission.Index,
                        submission.Student,
                        submission.ActivityName,
                        submission.ActivityType,
                        submission.AssignmentKey,
                        submission.SubmittedAt,
                        submission.EvaluationUrl,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        Array.Empty<string>(),
                        string.Empty,
                        "Missing evaluation URL."));
                    continue;
                }

                ReportActivity($"Scraping submission {i + 1} of {targetSubmissions.Count}: {submission.Student} - {submission.ActivityName}");

                var detailPage = await context.NewPageAsync();
                BrightspaceSubmissionDetail? detail = null;
                string? error = null;

                try
                {
                    detail = await ScrapeSubmissionDetailAsync(detailPage, submission.EvaluationUrl);
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }
                finally
                {
                    await detailPage.CloseAsync();
                }

                entries.Add(new BrightspaceSubmissionMapEntry(
                    submission.Index,
                    submission.Student,
                    submission.ActivityName,
                    submission.ActivityType,
                    submission.AssignmentKey,
                    submission.SubmittedAt,
                    submission.EvaluationUrl,
                    detail?.PageTitle,
                    detail?.PreviewUrl,
                    detail?.RepoUrl,
                    detail?.Owner,
                    detail?.Repo,
                    detail?.CloneUrl,
                    detail?.BranchHint,
                    detail?.SubdirHint,
                    detail?.AssignmentPathHint,
                    detail?.Urls ?? Array.Empty<string>(),
                    detail?.RawText ?? string.Empty,
                    error));
            }

            var result = new BrightspaceSubmissionMapResult(
                ArtifactSchemaVersion,
                ScraperName,
                DateTimeOffset.UtcNow,
                quickEvalUrl,
                quickEvalSubmissions.Count,
                entries.Count,
                entries);

            await WriteJsonAsync(outPath, result, cancellationToken);
            ReportActivity($"Wrote {entries.Count} merged submissions to {outPath}");
            return result;
        }
        catch (Exception ex)
        {
            ReportActivity(ex.Message, true);
            throw;
        }
        finally
        {
            gate.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await gate.WaitAsync();

        try
        {
            await DisposeLoginResourcesAsync();
        }
        finally
        {
            gate.Release();
            gate.Dispose();
        }
    }

    private static async Task WriteJsonAsync<T>(string path, T value, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(value, SerializerOptions), cancellationToken);
    }

    private async Task DisposeLoginResourcesAsync()
    {
        if (loginContext is not null)
        {
            await loginContext.CloseAsync();
            loginContext = null;
        }

        if (browser is not null)
        {
            await browser.CloseAsync();
            browser = null;
        }

        playwright?.Dispose();
        playwright = null;
    }

    private static async Task<List<BrightspaceQuickEvalSubmission>> ScrapeQuickEvalSubmissionsAsync(IPage page, string url, bool scrapeAllPages, CancellationToken cancellationToken)
    {
        await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        var submissions = new List<BrightspaceQuickEvalSubmission>();
        var seenEvaluationUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pageNumber = 1;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var currentPageRows = await ReadQuickEvalRowsAsync(page, url);

            foreach (var row in currentPageRows)
            {
                if (string.IsNullOrWhiteSpace(row.EvaluationUrl) || seenEvaluationUrls.Add(row.EvaluationUrl))
                {
                    submissions.Add(row with { Index = submissions.Count });
                }
            }

            if (!scrapeAllPages)
            {
                break;
            }

            var moved = await TryAdvanceQuickEvalPageAsync(page, pageNumber);
            if (!moved)
            {
                break;
            }

            pageNumber++;
        }

        return submissions;
    }

    private static async Task<List<BrightspaceCourseOption>> ReadCourseCatalogAsync(IPage page, string baseUrl)
    {
        var baseUri = new Uri(baseUrl, UriKind.Absolute);
        var selector = "a[href*='/d2l/home/'], a[href*='/d2l/home?ou='], a[href*='/d2l/home/?ou=']";
        var anchors = page.Locator(selector);
        var count = await anchors.CountAsync();
        var results = new List<BrightspaceCourseOption>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < count; i++)
        {
            var anchor = anchors.Nth(i);
            var href = await anchor.GetAttributeAsync("href");
            var resolvedUrl = ToAbsoluteUrl(baseUri, href);

            if (string.IsNullOrWhiteSpace(resolvedUrl))
            {
                continue;
            }

            var courseId = TryExtractCourseId(resolvedUrl);
            if (string.IsNullOrWhiteSpace(courseId))
            {
                continue;
            }

            var text = NormalizeWhitespace(await anchor.InnerTextAsync());
            var title = NormalizeWhitespace(await anchor.GetAttributeAsync("title"));
            var ariaLabel = NormalizeWhitespace(await anchor.GetAttributeAsync("aria-label"));
            var label = FirstNonEmpty(text, title, ariaLabel);
            if (string.IsNullOrWhiteSpace(label))
            {
                continue;
            }

            var key = $"{courseId}|{label}".ToLowerInvariant();
            if (!seen.Add(key))
            {
                continue;
            }

            var courseCode = TryReadCourseCode(label);
            var termLabel = BrightspaceTermLabelParser.ParseOrDefault(label);
            results.Add(new BrightspaceCourseOption(
                label,
                resolvedUrl,
                courseId,
                termLabel,
                courseCode));
        }

        return results
            .OrderBy(static course => course.TermLabel, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static course => course.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static async Task<List<BrightspaceCourseOption>> ReadCourseCatalogAcrossTermsAsync(IPage page, string baseUrl)
    {
        var merged = MergeCourses(Array.Empty<BrightspaceCourseOption>(), await ReadCourseCatalogAsync(page, baseUrl));
        var termFilters = await ReadAvailableTermFilterLabelsAsync(page);

        foreach (var termFilter in termFilters)
        {
            if (!await ActivateTermFilterAsync(page, termFilter))
            {
                continue;
            }

            await WaitForCourseListRefreshAsync(page);
            var visibleCourses = await ReadCourseCatalogAsync(page, baseUrl);

            if (visibleCourses.Count == 0)
            {
                continue;
            }

            var relabeledCourses = visibleCourses
                .Select(course => course with
                {
                    TermLabel = NormalizeTermFilterLabel(termFilter, course.TermLabel),
                })
                .ToArray();

            merged = MergeCourses(merged, relabeledCourses);
        }

        return merged
            .OrderBy(static course => course.TermLabel, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static course => course.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static async Task<List<BrightspaceQuickEvalSubmission>> ReadQuickEvalRowsAsync(IPage page, string url)
    {
        var pageUri = new Uri(url, UriKind.Absolute);
        var quickEvalState = await WaitForQuickEvalStateAsync(page);

        if (quickEvalState.IsEmpty)
        {
            return [];
        }

        if (!quickEvalState.HasRows)
        {
            throw new InvalidOperationException(quickEvalState.DiagnosticMessage);
        }

        var rows = page.Locator("d2l-quick-eval-submissions-table tbody > tr");
        var rowCount = await rows.CountAsync();
        var submissions = new List<BrightspaceQuickEvalSubmission>();

        for (var i = 0; i < rowCount; i++)
        {
            var row = rows.Nth(i);
            var nameNode = row.Locator("d2l-link.d2l-quick-eval-submissions-table-name-link");
            var activityNode = row.Locator("d2l-activity-name .d2l-activity-name-text");
            var dateNode = row.Locator("td.d2l-quick-eval-submissions-overflow-hidden span");
            var evaluationNode = row.Locator("d2l-link.d2l-quick-eval-submissions-table-name-link a");

            var student = await ReadNamedValueAsync(nameNode);
            var activityName = await ReadNamedValueAsync(activityNode);
            var submittedAt = await ReadNamedValueAsync(dateNode);
            var evaluationUrl = ToAbsoluteUrl(pageUri, await ReadHrefAsync(evaluationNode));
            var activityType = BrightspaceAssignmentClassifier.GetActivityType(activityName);
            var assignmentKey = BrightspaceAssignmentClassifier.GetAssignmentKey(activityName, activityType);

            submissions.Add(new BrightspaceQuickEvalSubmission(
                i,
                student,
                activityName,
                activityType,
                assignmentKey,
                submittedAt,
                evaluationUrl,
                [.. new[] { evaluationUrl }.Where(static value => !string.IsNullOrWhiteSpace(value))!]));
        }

        return submissions;
    }

    private static async Task<QuickEvalPageState> WaitForQuickEvalStateAsync(IPage page)
    {
        const int timeoutMs = 30000;

        try
        {
            await page.WaitForFunctionAsync(
                """
                () => {
                  const rowCount = document.querySelectorAll('d2l-quick-eval-submissions-table tbody > tr').length;
                  if (rowCount > 0) {
                    return 'rows';
                  }

                  const bodyText = (document.body?.innerText || '').toLowerCase();
                  const emptyHints = [
                    'no submissions to evaluate',
                    'nothing to evaluate',
                    'no activities to evaluate',
                    'no items to evaluate',
                    'no quick eval items',
                    'you have no submissions',
                  ];

                  if (emptyHints.some(hint => bodyText.includes(hint))) {
                    return 'empty';
                  }

                  const likelyLogin =
                    bodyText.includes('sign in') ||
                    bodyText.includes('log in') ||
                    bodyText.includes('username') ||
                    bodyText.includes('password');

                  if (likelyLogin) {
                    return 'login';
                  }

                  return '';
                }
                """,
                null,
                new PageWaitForFunctionOptions { Timeout = timeoutMs });
        }
        catch (TimeoutException)
        {
            return await BuildQuickEvalDiagnosticStateAsync(page, "Timed out waiting for the Quick Eval page to show rows or an empty-state message.");
        }

        var state = await page.EvaluateAsync<string>(
            """
            () => {
              const rowCount = document.querySelectorAll('d2l-quick-eval-submissions-table tbody > tr').length;
              if (rowCount > 0) {
                return 'rows';
              }

              const bodyText = (document.body?.innerText || '').toLowerCase();
              const emptyHints = [
                'no submissions to evaluate',
                'nothing to evaluate',
                'no activities to evaluate',
                'no items to evaluate',
                'no quick eval items',
                'you have no submissions',
              ];

              if (emptyHints.some(hint => bodyText.includes(hint))) {
                return 'empty';
              }

              const likelyLogin =
                bodyText.includes('sign in') ||
                bodyText.includes('log in') ||
                bodyText.includes('username') ||
                bodyText.includes('password');

              if (likelyLogin) {
                return 'login';
              }

              return '';
            }
            """);

        return state switch
        {
            "rows" => new QuickEvalPageState(true, false, null),
            "empty" => new QuickEvalPageState(false, true, null),
            "login" => await BuildQuickEvalDiagnosticStateAsync(page, "The saved Brightspace session appears to have landed on a login page instead of an authenticated Quick Eval page."),
            _ => await BuildQuickEvalDiagnosticStateAsync(page, "The Quick Eval page loaded, but the expected submission rows were not detected."),
        };
    }

    private static async Task<QuickEvalPageState> BuildQuickEvalDiagnosticStateAsync(IPage page, string prefix)
    {
        var title = await page.TitleAsync();
        var currentUrl = page.Url;
        var rowCount = await page.Locator("d2l-quick-eval-submissions-table tbody > tr").CountAsync();
        var tableCount = await page.Locator("d2l-quick-eval-submissions-table").CountAsync();
        var bodySnippet = await page.EvaluateAsync<string>(
            """
            () => {
              const text = (document.body?.innerText || '').replace(/\s+/g, ' ').trim();
              return text.slice(0, 400);
            }
            """);

        var message = string.Join(
            Environment.NewLine,
            [
                prefix,
                $"Page title: {title}",
                $"Page URL: {currentUrl}",
                $"Quick Eval table count: {tableCount}",
                $"Quick Eval row count: {rowCount}",
                $"Body preview: {bodySnippet}",
            ]);

        return new QuickEvalPageState(false, false, message);
    }

    private static async Task<bool> TryAdvanceQuickEvalPageAsync(IPage page, int pageNumber)
    {
        var beforeSignature = await GetQuickEvalPageSignatureAsync(page);
        var beforeRowCount = await GetQuickEvalRowCountAsync(page);
        var candidates = new[]
        {
            "d2l-button.d2l-quick-eval-submissions-table-load-more",
            "button:has-text('Load More')",
            "a:has-text('Load More')",
            "button:has-text('Load more')",
            "a:has-text('Load more')",
            "button:has-text('Next')",
            "a:has-text('Next')",
            "[aria-label='Next']",
            "[title='Next']",
        };

        foreach (var selector in candidates)
        {
            var candidate = page.Locator(selector).First;
            if (!await IsActionableAsync(candidate))
            {
                continue;
            }

            await candidate.ClickAsync();

            try
            {
                await page.WaitForFunctionAsync(
                    """
                    previousRowCount => {
                      const rows = document.querySelectorAll('d2l-quick-eval-submissions-table tbody > tr');
                      return rows.length > previousRowCount;
                    }
                    """,
                    beforeRowCount,
                    new PageWaitForFunctionOptions { Timeout = 5000 });
            }
            catch (TimeoutException)
            {
                var afterRowCount = await GetQuickEvalRowCountAsync(page);
                var afterSignature = await GetQuickEvalPageSignatureAsync(page);
                if (afterRowCount <= beforeRowCount && afterSignature == beforeSignature)
                {
                    continue;
                }
            }

            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            return true;
        }

        return false;
    }

    private static async Task<int> GetQuickEvalRowCountAsync(IPage page)
        => await page.Locator("d2l-quick-eval-submissions-table tbody > tr").CountAsync();

    private static async Task<string> GetQuickEvalPageSignatureAsync(IPage page)
        => await page.EvaluateAsync<string>(
            """
            () => {
              const rows = [...document.querySelectorAll('d2l-quick-eval-submissions-table tbody > tr')];
              const links = rows.map(row => {
                const anchor = row.querySelector('d2l-link.d2l-quick-eval-submissions-table-name-link a');
                return anchor?.getAttribute('href') || '';
              });

              return links.join('|');
            }
            """);

    private static async Task<bool> IsActionableAsync(ILocator locator)
    {
        if (await locator.CountAsync() == 0 || !await locator.IsVisibleAsync())
        {
            return false;
        }

        if (await locator.IsDisabledAsync())
        {
            return false;
        }

        var ariaDisabled = await locator.GetAttributeAsync("aria-disabled");
        return !string.Equals(ariaDisabled, "true", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<BrightspaceSubmissionDetail> ScrapeSubmissionDetailAsync(IPage page, string url)
    {
        var pageUri = new Uri(url, UriKind.Absolute);
        await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var title = await page.TitleAsync();
        var bodyText = await ReadSubmissionTextAsync(page);
        var previewUrl = await ReadPreviewUrlAsync(page, pageUri);
        var links = await page.Locator("a[href]").EvaluateAllAsync<string[]>(
            "nodes => nodes.map(n => n.href).filter(Boolean)");

        var uniqueLinks = links
            .Where(static value => IsUsefulUrl(value))
            .Where(static value => !IsQuickEvalReturnUrl(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var repoUrl = uniqueLinks.FirstOrDefault(static value =>
            value.Contains("github.com/", StringComparison.OrdinalIgnoreCase));

        var github = BrightspaceGitHubHintParser.Parse(repoUrl);
        return new BrightspaceSubmissionDetail(
            ScraperName,
            DateTimeOffset.UtcNow,
            title,
            url,
            previewUrl,
            repoUrl,
            github.Owner,
            github.Repo,
            github.CloneUrl,
            github.BranchHint,
            github.SubdirHint,
            BrightspaceAssignmentPathHintParser.Parse(bodyText),
            uniqueLinks,
            bodyText);
    }

    private static async Task<string?> ReadNamedValueAsync(ILocator locator)
    {
        if (await locator.CountAsync() == 0)
        {
            return null;
        }

        var title = await locator.First.GetAttributeAsync("title");
        if (!string.IsNullOrWhiteSpace(title))
        {
            return StripEvaluatePrefix(title);
        }

        var aria = await locator.First.GetAttributeAsync("aria-label");
        if (!string.IsNullOrWhiteSpace(aria))
        {
            return StripEvaluatePrefix(aria);
        }

        var text = (await locator.First.InnerTextAsync()).Trim();
        return string.IsNullOrWhiteSpace(text) ? null : StripEvaluatePrefix(text);
    }

    private static async Task<string?> ReadHrefAsync(ILocator locator)
    {
        if (await locator.CountAsync() == 0)
        {
            return null;
        }

        return await locator.First.GetAttributeAsync("href");
    }

    private static string StripEvaluatePrefix(string value)
        => value.StartsWith("Evaluate ", StringComparison.OrdinalIgnoreCase)
            ? value["Evaluate ".Length..].Trim()
            : value.Trim();

    private static string? ToAbsoluteUrl(Uri pageUri, string? href)
    {
        if (string.IsNullOrWhiteSpace(href))
        {
            return null;
        }

        return Uri.TryCreate(pageUri, href, out var absoluteUri)
            ? absoluteUri.ToString()
            : href;
    }

    private static bool IsUsefulUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return !value.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase)
            && !value.StartsWith("about:", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsQuickEvalReturnUrl(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return uri.AbsolutePath.Contains("/quickeval/", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<string> ReadSubmissionTextAsync(IPage page)
    {
        var submissionItem = page.Locator("d2l-consistent-evaluation-assignments-submission-item");
        if (await submissionItem.CountAsync() > 0)
        {
            var commentHtml = await submissionItem.First.GetAttributeAsync("comment");
            var commentText = ExtractTextFromHtml(commentHtml);
            if (!string.IsNullOrWhiteSpace(commentText))
            {
                return commentText;
            }
        }

        var submissionBlock = page.Locator("d2l-html-block.d2l-submission-item-text");
        if (await submissionBlock.CountAsync() > 0)
        {
            var html = await submissionBlock.First.GetAttributeAsync("html");
            var text = ExtractTextFromHtml(html);
            if (!string.IsNullOrWhiteSpace(text))
            {
                return text;
            }
        }

        var selectors = new[]
        {
            "d2l-html-block",
            "d2l-assignment-submission-view",
            "d2l-assignment-evaluation",
            "main",
            "body",
        };

        foreach (var selector in selectors)
        {
            var locator = page.Locator(selector);
            if (await locator.CountAsync() == 0)
            {
                continue;
            }

            var text = NormalizeWhitespace(await locator.First.InnerTextAsync());
            if (!string.IsNullOrWhiteSpace(text))
            {
                return text;
            }
        }

        var fullText = await page.EvaluateAsync<string>(
            """
            () => {
              const seen = new Set();
              const parts = [];
              const nodes = document.querySelectorAll('textarea, input[type="text"], [data-automation], [aria-label], [title]');

              for (const node of nodes) {
                const candidates = [];

                if (node instanceof HTMLTextAreaElement || node instanceof HTMLInputElement) {
                  candidates.push(node.value);
                }

                candidates.push(node.getAttribute('data-automation'));
                candidates.push(node.getAttribute('aria-label'));
                candidates.push(node.getAttribute('title'));
                candidates.push(node.textContent);

                for (const candidate of candidates) {
                  const value = candidate?.trim();
                  if (!value || seen.has(value)) {
                    continue;
                  }

                  seen.add(value);
                  parts.push(value);
                }
              }

              return parts.join('\n');
            }
            """);

        return NormalizeWhitespace(fullText);
    }

    private static async Task<string?> ReadPreviewUrlAsync(IPage page, Uri pageUri)
    {
        var selectors = new[]
        {
            "d2l-consistent-evaluation-page",
            "consistent-evaluation-right-panel",
        };

        foreach (var selector in selectors)
        {
            var locator = page.Locator(selector);
            if (await locator.CountAsync() == 0)
            {
                continue;
            }

            var previewPath = await locator.First.GetAttributeAsync("preview-activity-path");
            var previewUrl = ToAbsoluteUrl(pageUri, previewPath);
            if (!string.IsNullOrWhiteSpace(previewUrl))
            {
                return previewUrl;
            }
        }

        return null;
    }

    private static string ExtractTextFromHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var decoded = System.Net.WebUtility.HtmlDecode(html);
        if (string.IsNullOrWhiteSpace(decoded))
        {
            return string.Empty;
        }

        var withLineBreaks = Regex.Replace(decoded, @"<(br|/p|/div|/li)\b[^>]*>", Environment.NewLine, RegexOptions.IgnoreCase);
        var withoutTags = Regex.Replace(withLineBreaks, "<[^>]+>", " ");
        return NormalizeWhitespace(withoutTags);
    }

    private static string NormalizeWhitespace(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return string.Join(
            Environment.NewLine,
            value.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(static line => !string.IsNullOrWhiteSpace(line)));
    }

    private static async Task<InvalidOperationException> BuildCourseDiscoveryExceptionAsync(IPage page, string prefix)
    {
        var title = await page.TitleAsync();
        var currentUrl = page.Url;
        var anchorCount = await page.Locator("a[href]").CountAsync();
        var candidateLinks = await ReadCandidateCourseLinksAsync(page);
        var bodySnippet = await page.EvaluateAsync<string>(
            """
            () => {
              const text = (document.body?.innerText || '').replace(/\s+/g, ' ').trim();
              return text.slice(0, 500);
            }
            """);

        var message = string.Join(
            Environment.NewLine,
            [
                prefix,
                $"Page title: {title}",
                $"Page URL: {currentUrl}",
                $"Anchor count: {anchorCount}",
                $"Candidate links: {(candidateLinks.Count == 0 ? "none" : string.Join(" | ", candidateLinks))}",
                $"Body preview: {bodySnippet}",
            ]);

        return new InvalidOperationException(message);
    }

    private static string? TryReadCourseCode(string? courseName)
    {
        if (string.IsNullOrWhiteSpace(courseName))
        {
            return null;
        }

        var match = Regex.Match(courseName, @"\b[A-Z]{3,5}\s*\d{3,5}\b", RegexOptions.IgnoreCase);
        return match.Success ? match.Value.Trim() : null;
    }

    private static string? TryExtractCourseId(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return null;
        }

        var pathMatch = Regex.Match(uri.AbsolutePath, @"/d2l/home/(\d+)", RegexOptions.IgnoreCase);
        if (pathMatch.Success)
        {
            return pathMatch.Groups[1].Value;
        }

        var query = uri.Query.TrimStart('?');
        if (string.IsNullOrWhiteSpace(query))
        {
            return null;
        }

        foreach (var part in query.Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var pieces = part.Split('=', 2);
            if (!string.Equals(pieces[0], "ou", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (pieces.Length < 2 || string.IsNullOrWhiteSpace(pieces[1]))
            {
                return null;
            }

            return Uri.UnescapeDataString(pieces[1]).Trim();
        }

        return null;
    }

    private static string? FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

    private static List<BrightspaceCourseOption> MergeCourses(IEnumerable<BrightspaceCourseOption> existing, IEnumerable<BrightspaceCourseOption> additional)
    {
        var merged = new Dictionary<string, BrightspaceCourseOption>(StringComparer.OrdinalIgnoreCase);

        foreach (var course in existing.Concat(additional))
        {
            var key = $"{course.CourseId}|{course.Name}";

            if (!merged.TryGetValue(key, out var current))
            {
                merged[key] = course;
                continue;
            }

            var replacement = current with
            {
                Url = string.IsNullOrWhiteSpace(current.Url) ? course.Url : current.Url,
                CourseCode = string.IsNullOrWhiteSpace(current.CourseCode) ? course.CourseCode : current.CourseCode,
                TermLabel = PreferMoreSpecificTerm(current.TermLabel, course.TermLabel),
            };

            merged[key] = replacement;
        }

        return merged.Values.ToList();
    }

    private static string PreferMoreSpecificTerm(string current, string candidate)
    {
        if (string.IsNullOrWhiteSpace(current) || string.Equals(current, "Uncategorized", StringComparison.OrdinalIgnoreCase))
        {
            return candidate;
        }

        if (string.IsNullOrWhiteSpace(candidate) || string.Equals(candidate, "Uncategorized", StringComparison.OrdinalIgnoreCase))
        {
            return current;
        }

        return current;
    }

    private static string NormalizeTermFilterLabel(string termFilter, string parsedTermLabel)
    {
        if (string.IsNullOrWhiteSpace(termFilter)
            || string.Equals(termFilter, "All", StringComparison.OrdinalIgnoreCase)
            || string.Equals(termFilter, "All Terms", StringComparison.OrdinalIgnoreCase)
            || string.Equals(termFilter, "All Semesters", StringComparison.OrdinalIgnoreCase))
        {
            return parsedTermLabel;
        }

        return termFilter;
    }

    private static async Task<List<string>> ReadAvailableTermFilterLabelsAsync(IPage page)
    {
        return await page.EvaluateAsync<List<string>>(
            """
            () => {
              const results = [];
              const seen = new Set();
              const termRegex = /\b(?:spring|summer|fall|autumn|winter)\s+20\d{2}\b|\b20\d{2}\s+(?:spring|summer|fall|autumn|winter)\b/i;
              const genericRegex = /^(all|all terms|all semesters)$/i;
              const selectors = [
                'select option',
                '[role="tab"]',
                'button',
                '[role="button"]',
                'a',
              ];

              for (const selector of selectors) {
                for (const node of document.querySelectorAll(selector)) {
                  const text = (node.textContent || '').replace(/\s+/g, ' ').trim();
                  if (!text || text.length > 40) {
                    continue;
                  }

                  if (!termRegex.test(text) && !genericRegex.test(text)) {
                    continue;
                  }

                  const key = text.toLowerCase();
                  if (seen.has(key)) {
                    continue;
                  }

                  seen.add(key);
                  results.push(text);
                }
              }

              return results;
            }
            """);
    }

    private static async Task<bool> ActivateTermFilterAsync(IPage page, string label)
    {
        if (await TryActivateSelectOptionAsync(page, label))
        {
            return true;
        }

        var tabLocator = page.GetByRole(AriaRole.Tab, new() { Name = label, Exact = true }).First;
        if (await IsActionableAsync(tabLocator))
        {
            await tabLocator.ClickAsync();
            return true;
        }

        var buttonLocator = page.GetByRole(AriaRole.Button, new() { Name = label, Exact = true }).First;
        if (await IsActionableAsync(buttonLocator))
        {
            await buttonLocator.ClickAsync();
            return true;
        }

        var fallbackLocator = page.GetByText(label, new() { Exact = true }).First;
        if (await IsActionableAsync(fallbackLocator))
        {
            await fallbackLocator.ClickAsync();
            return true;
        }

        return false;
    }

    private static async Task<bool> TryActivateSelectOptionAsync(IPage page, string label)
    {
        var selects = page.Locator("select");
        var selectCount = await selects.CountAsync();

        for (var i = 0; i < selectCount; i++)
        {
            var select = selects.Nth(i);
            var optionValues = await select.EvaluateAsync<string[]>(
                """
                (element, desiredLabel) => {
                  if (!(element instanceof HTMLSelectElement)) {
                    return [];
                  }

                  return [...element.options]
                    .filter(option => (option.textContent || '').replace(/\s+/g, ' ').trim() === desiredLabel)
                    .map(option => option.value);
                }
                """,
                label);

            if (optionValues.Length == 0)
            {
                continue;
            }

            await select.SelectOptionAsync(new[] { optionValues[0] });
            return true;
        }

        return false;
    }

    private static async Task WaitForCourseListRefreshAsync(IPage page)
    {
        try
        {
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
        catch
        {
        }

        await page.WaitForTimeoutAsync(750);
    }

    private static async Task<List<string>> ReadCandidateCourseLinksAsync(IPage page)
    {
        var selector = "a[href*='/d2l/home/'], a[href*='/d2l/home?ou='], a[href*='/d2l/home/?ou=']";
        var anchors = page.Locator(selector);
        var count = Math.Min(await anchors.CountAsync(), 12);
        var results = new List<string>();

        for (var i = 0; i < count; i++)
        {
            var anchor = anchors.Nth(i);
            var href = await anchor.GetAttributeAsync("href");
            var text = NormalizeWhitespace(await anchor.InnerTextAsync());
            var title = NormalizeWhitespace(await anchor.GetAttributeAsync("title"));
            var label = FirstNonEmpty(text, title, href);

            if (!string.IsNullOrWhiteSpace(label))
            {
                results.Add(label);
            }
        }

        return results;
    }

    private static async Task<PlaywrightBrowser> LaunchBrowserAsync(IPlaywright playwright, string? channel, bool headless)
    {
        var resolvedChannel = string.IsNullOrWhiteSpace(channel)
            ? (OperatingSystem.IsWindows() ? "msedge" : "chrome")
            : channel.Trim();

        try
        {
            return await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = headless,
                Channel = resolvedChannel,
            });
        }
        catch (PlaywrightException ex) when (!string.IsNullOrWhiteSpace(resolvedChannel))
        {
            throw new InvalidOperationException(
                $"Failed to launch browser channel '{resolvedChannel}'. Set the Brightspace browser channel to 'chrome' or 'msedge' in Settings and ensure that browser is installed.",
                ex);
        }
    }

    private void ReportActivity(string message, bool isError = false)
    {
        ActivityReported?.Invoke(this, new BrightspaceActivityEventArgs(message, isError));
    }

    private sealed record QuickEvalPageState(bool HasRows, bool IsEmpty, string? DiagnosticMessage);

}
