using System.Text;
using System.Text.Json;
using Microsoft.Playwright;

var options = ProbeOptions.Parse(args);
Directory.CreateDirectory(options.OutputDirectory);

var runFolder = Path.Combine(
    options.OutputDirectory,
    $"run-{DateTimeOffset.Now:yyyyMMdd-HHmmss}");
Directory.CreateDirectory(runFolder);

var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true,
};

Console.WriteLine($"Base URL: {options.BaseUrl}");
Console.WriteLine($"Session: {options.SessionStatePath}");
Console.WriteLine($"Output: {runFolder}");

if (!File.Exists(options.SessionStatePath))
{
    Console.Error.WriteLine($"Session file not found: {options.SessionStatePath}");
    return 1;
}

var driverPath = ResolvePlaywrightDriverSearchPath();
if (!string.IsNullOrWhiteSpace(driverPath))
{
    Environment.SetEnvironmentVariable("PLAYWRIGHT_DRIVER_SEARCH_PATH", driverPath);
    Console.WriteLine($"PLAYWRIGHT_DRIVER_SEARCH_PATH = {driverPath}");
}

using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
{
    Headless = options.Headless,
    Channel = options.Channel,
});

await using var context = await browser.NewContextAsync(new BrowserNewContextOptions
{
    StorageStatePath = options.SessionStatePath,
});

var page = await context.NewPageAsync();
await page.GotoAsync(options.BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
await page.WaitForTimeoutAsync(1500);

var html = await page.ContentAsync();
await File.WriteAllTextAsync(Path.Combine(runFolder, "page.html"), html, Encoding.UTF8);

var visibleText = await page.EvaluateAsync<string>(
    """
    () => (document.body?.innerText || '').replace(/\r/g, '').trim()
    """);
await File.WriteAllTextAsync(Path.Combine(runFolder, "page.txt"), visibleText ?? string.Empty, Encoding.UTF8);

var snapshotJson = await page.EvaluateAsync<string>(
    """
    () => {
      const normalize = value => (value || '').toString().replace(/\s+/g, ' ').trim();

      const queryAllDeep = (selector, root = document) => {
        const results = [];
        const visit = currentRoot => {
          if (!currentRoot || typeof currentRoot.querySelectorAll !== 'function') {
            return;
          }

          results.push(...currentRoot.querySelectorAll(selector));

          for (const element of currentRoot.querySelectorAll('*')) {
            if (element.shadowRoot) {
              visit(element.shadowRoot);
            }
          }
        };

        visit(root);
        return results;
      };

      const widget = document.querySelector('d2l-my-courses-v2');
      const widgetAttributes = widget
        ? Object.fromEntries([...widget.attributes].map(attr => [attr.name, attr.value]))
        : {};

      const tabs = queryAllDeep("d2l-tab[role='tab'], [role='tab']")
        .map(tab => ({
          id: tab.getAttribute('id') || '',
          title: normalize(tab.getAttribute('title')) || normalize(tab.textContent),
          selected: tab.hasAttribute('selected') || tab.getAttribute('aria-selected') === 'true',
          ariaControls: tab.getAttribute('aria-controls') || '',
        }))
        .filter(tab => tab.title);

      const cards = queryAllDeep('d2l-my-courses-enrollment-card')
        .map(card => {
          const cardRoot = card.shadowRoot || card;
          const cardNode = queryAllDeep('d2l-card', cardRoot)[0];
          const linkNode = queryAllDeep("a[href*='/d2l/home/'], d2l-card[href*='/d2l/home/']", cardRoot)[0] || cardNode;
          const id = normalize(card.getAttribute('id'));
          const name = normalize(queryAllDeep('.d2l-organization-name', cardRoot)[0]?.textContent);
          const text = normalize(cardNode?.getAttribute('text'));
          const href = normalize(linkNode?.getAttribute('href'));
          const metadata = queryAllDeep('d2l-object-property-list-item', cardRoot)
            .map(node => normalize(node.textContent))
            .filter(Boolean);

          return {
            id,
            name,
            text,
            href,
            metadata,
          };
        });

      return JSON.stringify({
        title: document.title,
        url: window.location.href,
        widgetAttributes,
        tabCount: tabs.length,
        cardCount: cards.length,
        tabs,
        cards,
      }, null, 2);
    }
    """);

var apiSnapshotJson = await page.EvaluateAsync<string>(
    """
    async () => {
      const normalize = value => (value || '').toString().trim();
      const widget = document.querySelector('d2l-my-courses-v2');
      if (!widget) {
        return JSON.stringify({
          semestersUrl: '',
          coursesUrl: '',
          semestersJson: '',
          coursesJson: '',
        }, null, 2);
      }

      const toAbsoluteUrl = value => {
        const raw = normalize(value);
        if (!raw) {
          return '';
        }

        try {
          return new URL(raw, window.location.origin).toString();
        } catch {
          return '';
        }
      };

      const fetchPretty = async url => {
        if (!url) {
          return '';
        }

        try {
          const response = await fetch(url, {
            credentials: 'include',
            headers: { 'accept': 'application/json, text/plain, */*' },
          });

          const body = await response.text();
          try {
            return JSON.stringify(JSON.parse(body), null, 2);
          } catch {
            return body;
          }
        } catch (error) {
          return `FETCH FAILED: ${error}`;
        }
      };

      const fetchJson = async url => {
        if (!url) {
          return null;
        }

        try {
          const response = await fetch(url, {
            credentials: 'include',
            headers: { 'accept': 'application/json, text/plain, */*' },
          });

          if (!response.ok) {
            return { error: `HTTP ${response.status}`, url };
          }

          return await response.json();
        } catch (error) {
          return { error: String(error), url };
        }
      };

      const fetchAllCoursePages = async url => {
        const pages = [];
        const allCourses = [];
        const seenBookmarks = new Set();
        let nextUrl = url;

        while (nextUrl) {
          const payload = await fetchJson(nextUrl);
          pages.push({
            url: nextUrl,
            count: Array.isArray(payload?.Courses) ? payload.Courses.length : 0,
            bookmark: normalize(payload?.Bookmark),
            error: normalize(payload?.error),
          });

          if (!payload || !Array.isArray(payload.Courses)) {
            break;
          }

          allCourses.push(...payload.Courses);

          const bookmark = normalize(payload.Bookmark);
          if (!bookmark || seenBookmarks.has(bookmark)) {
            break;
          }

          seenBookmarks.add(bookmark);
          const parsedUrl = new URL(nextUrl, window.location.origin);
          parsedUrl.searchParams.set('bookmark', bookmark);
          nextUrl = parsedUrl.toString();
        }

        return {
          pages,
          courses: allCourses,
        };
      };

      const semestersUrl = toAbsoluteUrl(widget.getAttribute('semesters-url'));
      const coursesUrl = toAbsoluteUrl(widget.getAttribute('enrollments-url'));
      const allCoursesResult = await fetchAllCoursePages(coursesUrl);

      return JSON.stringify({
        semestersUrl,
        coursesUrl,
        semestersJson: await fetchPretty(semestersUrl),
        coursesJson: await fetchPretty(coursesUrl),
        allCoursesJson: JSON.stringify({
          Courses: allCoursesResult.courses,
          PageDiagnostics: allCoursesResult.pages,
          TotalCourses: allCoursesResult.courses.length,
        }, null, 2),
      }, null, 2);
    }
    """);

await File.WriteAllTextAsync(
    Path.Combine(runFolder, "snapshot.json"),
    snapshotJson ?? "{}",
    Encoding.UTF8);

var snapshotDocument = JsonDocument.Parse(snapshotJson ?? "{}");
var snapshotRoot = snapshotDocument.RootElement;
var widgetAttributesJson = snapshotRoot.TryGetProperty("widgetAttributes", out var widgetAttributesElement)
    ? JsonSerializer.Serialize(widgetAttributesElement, jsonOptions)
    : "{}";
var tabsJson = snapshotRoot.TryGetProperty("tabs", out var tabsElement)
    ? JsonSerializer.Serialize(tabsElement, jsonOptions)
    : "[]";
var cardsJson = snapshotRoot.TryGetProperty("cards", out var cardsElement)
    ? JsonSerializer.Serialize(cardsElement, jsonOptions)
    : "[]";

await File.WriteAllTextAsync(
    Path.Combine(runFolder, "widget-attributes.json"),
    widgetAttributesJson,
    Encoding.UTF8);

await File.WriteAllTextAsync(
    Path.Combine(runFolder, "tabs.json"),
    tabsJson,
    Encoding.UTF8);

await File.WriteAllTextAsync(
    Path.Combine(runFolder, "cards.json"),
    cardsJson,
    Encoding.UTF8);

var apiDocument = JsonDocument.Parse(apiSnapshotJson ?? "{}");
var apiRoot = apiDocument.RootElement;
var semestersJson = apiRoot.TryGetProperty("semestersJson", out var semestersElement)
    ? semestersElement.GetString() ?? string.Empty
    : string.Empty;
var coursesJson = apiRoot.TryGetProperty("coursesJson", out var coursesElement)
    ? coursesElement.GetString() ?? string.Empty
    : string.Empty;
var allCoursesJson = apiRoot.TryGetProperty("allCoursesJson", out var allCoursesElement)
    ? allCoursesElement.GetString() ?? string.Empty
    : string.Empty;

await File.WriteAllTextAsync(
    Path.Combine(runFolder, "mysemesters.json"),
    semestersJson,
    Encoding.UTF8);

await File.WriteAllTextAsync(
    Path.Combine(runFolder, "mycourses.json"),
    coursesJson,
    Encoding.UTF8);

await File.WriteAllTextAsync(
    Path.Combine(runFolder, "mycourses-all-pages.json"),
    allCoursesJson,
    Encoding.UTF8);

var tabCount = snapshotRoot.TryGetProperty("tabCount", out var tabCountElement) ? tabCountElement.GetInt32() : 0;
var cardCount = snapshotRoot.TryGetProperty("cardCount", out var cardCountElement) ? cardCountElement.GetInt32() : 0;

Console.WriteLine($"Tabs: {tabCount}");
Console.WriteLine($"Cards: {cardCount}");
Console.WriteLine("Artifacts written:");
Console.WriteLine($"  {Path.Combine(runFolder, "page.html")}");
Console.WriteLine($"  {Path.Combine(runFolder, "page.txt")}");
Console.WriteLine($"  {Path.Combine(runFolder, "snapshot.json")}");
Console.WriteLine($"  {Path.Combine(runFolder, "widget-attributes.json")}");
Console.WriteLine($"  {Path.Combine(runFolder, "tabs.json")}");
Console.WriteLine($"  {Path.Combine(runFolder, "cards.json")}");
Console.WriteLine($"  {Path.Combine(runFolder, "mysemesters.json")}");
Console.WriteLine($"  {Path.Combine(runFolder, "mycourses.json")}");
Console.WriteLine($"  {Path.Combine(runFolder, "mycourses-all-pages.json")}");

return 0;

static string? ResolvePlaywrightDriverSearchPath()
{
    var appContextDirectory = AppContext.BaseDirectory;
    var candidates = new[]
    {
        appContextDirectory,
        Path.GetFullPath(Path.Combine(appContextDirectory, "..", "..", "..", "..")),
    };

    foreach (var candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
    {
        if (!Directory.Exists(candidate))
        {
            continue;
        }

        var hasPackage = File.Exists(Path.Combine(candidate, ".playwright", "package", "package.json"));
        var hasDriver = File.Exists(Path.Combine(candidate, "playwright.ps1"));
        if (hasPackage || hasDriver)
        {
            return candidate;
        }
    }

    return null;
}

internal sealed record ProbeOptions(
    string BaseUrl,
    string SessionStatePath,
    string OutputDirectory,
    string Channel,
    bool Headless)
{
    public static ProbeOptions Parse(string[] args)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var arg in args)
        {
            if (!arg.StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            var pieces = arg[2..].Split('=', 2);
            if (pieces.Length == 2)
            {
                values[pieces[0]] = pieces[1];
            }
        }

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var defaultSessionPath = Path.Combine(
            localAppData,
            "User Name",
            "com.instk.mauiclient",
            "Data",
            "Brightspace",
            "session.json");

        var outputDirectory = Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "BrightspaceProbeArtifacts");

        return new ProbeOptions(
            values.TryGetValue("base-url", out var baseUrl) ? baseUrl : "https://learn.cnm.edu",
            values.TryGetValue("session", out var sessionPath) ? sessionPath : Path.GetFullPath(defaultSessionPath),
            values.TryGetValue("out", out var outDir) ? Path.GetFullPath(outDir) : Path.GetFullPath(outputDirectory),
            values.TryGetValue("channel", out var channel) ? channel : "msedge",
            values.TryGetValue("headless", out var headless) && bool.TryParse(headless, out var parsedHeadless) ? parsedHeadless : true);
    }
}
