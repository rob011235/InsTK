using Microsoft.Playwright;
using System.Diagnostics;

namespace InsTK.MauiClient.Services;

public sealed class BrightspaceCourseScraper
{
    private const string BrightspaceHomeUrl = "https://mycourses.cnm.edu/d2l/home";

    public async Task<string> GetFullPageHtmlAsync(CancellationToken cancellationToken = default)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new()
        {
            Headless = false
        });

        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync(BrightspaceHomeUrl, new() { WaitUntil = WaitUntilState.DOMContentLoaded });

        // Log in manually, then hit "Resume" in the Playwright inspector
        await page.PauseAsync();

        var userId = 870;

        var waitUsers = page.WaitForResponseAsync(
            r => r.Ok && r.Url.Contains($".enrollments.api.brightspace.com/users/{userId}", StringComparison.OrdinalIgnoreCase),
            new() { Timeout = 120_000 }   // 2 minutes
        );

        // Trigger the request AFTER the waiter is armed
        await page.ReloadAsync(new() { WaitUntil = WaitUntilState.DOMContentLoaded });

        var response = await waitUsers;

        var json = await response.TextAsync();
        await File.WriteAllTextAsync("enrollments-users.json", json, cancellationToken);


        // Give the SPA a moment to finish rendering (tweak if needed)
        await page.WaitForTimeoutAsync(2000);

        // Full rendered DOM, not the original "view source"
        var bodyHtml = await page.EvaluateAsync<string>(
             "() => document.body.innerHTML"
         );

        return bodyHtml ?? string.Empty;
    }
}
