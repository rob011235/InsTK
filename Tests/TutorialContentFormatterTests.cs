using InsTK.Server.Components.Pages.Tutorials;
using InsTK.Shared.Models.Tutorials;
using Xunit;

namespace Tests;

public sealed class TutorialContentFormatterTests
{
    [Fact]
    public void BuildWordPressHtml_UsesStoredContentHtml()
    {
        var tutorial = new TutorialDefinition
        {
            Title = "Sample Tutorial",
            Technology = "ASP.NET Core",
            ContentHtml = "<h2>Step 1</h2><p>Do the thing.</p>"
        };

        var html = TutorialContentFormatter.BuildWordPressHtml(tutorial);

        Assert.Contains("<h2>Step 1</h2>", html);
        Assert.Contains("<p>Do the thing.</p>", html);
        Assert.DoesNotContain("Technology:", html);
    }

    [Fact]
    public void BuildWordPressHtml_IncludesYouTubeEmbedAfterSummary()
    {
        var tutorial = new TutorialDefinition
        {
            Title = "Sample Tutorial",
            Summary = "Learn the workflow.",
            YouTubeUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
            ContentHtml = "<h2>Step 1</h2><p>Do the thing.</p>"
        };

        var html = TutorialContentFormatter.BuildWordPressHtml(tutorial);
        var summaryIndex = html.IndexOf("<p>Learn the workflow.</p>", StringComparison.Ordinal);
        var iframeIndex = html.IndexOf("https://www.youtube.com/embed/dQw4w9WgXcQ", StringComparison.Ordinal);
        var contentIndex = html.IndexOf("<h2>Step 1</h2>", StringComparison.Ordinal);

        Assert.True(summaryIndex >= 0);
        Assert.True(iframeIndex > summaryIndex);
        Assert.True(contentIndex > iframeIndex);
    }

    [Fact]
    public void BuildWordPressHtml_NormalizesEmbedYouTubeUrl()
    {
        var tutorial = new TutorialDefinition
        {
            Title = "Sample Tutorial",
            YouTubeUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ"
        };

        var html = TutorialContentFormatter.BuildWordPressHtml(tutorial);

        Assert.Contains("https://www.youtube.com/embed/dQw4w9WgXcQ", html);
    }

    [Fact]
    public void BuildWordPressHtml_SkipsInvalidYouTubeUrl()
    {
        var tutorial = new TutorialDefinition
        {
            Title = "Sample Tutorial",
            Summary = "Learn the workflow.",
            YouTubeUrl = "https://example.com/not-youtube",
            ContentHtml = "<h2>Step 1</h2><p>Do the thing.</p>"
        };

        var html = TutorialContentFormatter.BuildWordPressHtml(tutorial);

        Assert.DoesNotContain("youtube.com/embed", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildBrightspaceAssignmentHtml_UsesAssignmentFieldsOnly()
    {
        var tutorial = new TutorialDefinition
        {
            Title = "Sample Tutorial",
            ContentHtml = "<p>This should never appear in Brightspace export.</p>",
            BrightspaceAssignmentInstructions = "Read this assignment carefully.",
            BrightspaceSubmissionInstructions = "Submit your repository URL.",
            BrightspacePoints = 75
        };

        var html = TutorialContentFormatter.BuildBrightspaceAssignmentHtml(tutorial);

        Assert.Contains("Read this assignment carefully.", html);
        Assert.Contains("Submit your repository URL.", html);
        Assert.Contains("75 points", html);
        Assert.DoesNotContain("This should never appear in Brightspace export.", html);
    }

    [Fact]
    public void BuildBrightspaceAssignmentHtml_GeneratesDefaultTitleWhenBlank()
    {
        var tutorial = new TutorialDefinition
        {
            Title = "Routing Basics",
            BrightspaceAssignmentTitle = null
        };

        var html = TutorialContentFormatter.BuildBrightspaceAssignmentHtml(tutorial);

        Assert.Contains("<h1>Tutorial - Routing Basics</h1>", html);
    }

    [Fact]
    public void BuildBrightspaceAssignmentHtml_ReplacesTutorialUrlPlaceholderUsingWordPressSlugConvention()
    {
        var tutorial = new TutorialDefinition
        {
            Title = "Installing Unity (Unity3D)",
            BrightspaceAssignmentInstructions = "Open {{url}} and finish the walkthrough."
        };

        var html = TutorialContentFormatter.BuildBrightspaceAssignmentHtml(tutorial);

        Assert.Contains("https://robgarnerblog.wordpress.com/tutorials/installing-unity-unity3d", html);
        Assert.Contains("target=\"_blank\"", html);
    }

    [Fact]
    public void SanitizeHtmlFragment_StripsUnsafeScriptAndEventHandlerMarkup()
    {
        const string html = """
        <script>alert('x')</script>
        <p onclick="alert('x')">Safe text</p>
        """;

        var sanitized = TutorialContentFormatter.SanitizeHtmlFragment(html);

        Assert.DoesNotContain("<script", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onclick", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<p>Safe text</p>", sanitized);
    }

    [Fact]
    public void SanitizeHtmlFragment_StripsJavascriptUrlsButPreservesSafeLinksAndImages()
    {
        const string html = """
        <a href="https://example.com/docs">Good</a>
        <a href="javascript:alert('x')">Bad</a>
        <img src="/uploads/example.png" alt="sample" onerror="alert('x')" />
        """;

        var sanitized = TutorialContentFormatter.SanitizeHtmlFragment(html);

        Assert.Contains("href=\"https://example.com/docs\"", sanitized);
        Assert.Contains("target=\"_blank\"", sanitized);
        Assert.DoesNotContain("javascript:alert", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("src=\"/uploads/example.png\"", sanitized);
        Assert.DoesNotContain("onerror", sanitized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildLegacyHtmlFromMarkdown_ConvertsExistingMarkdownForMigration()
    {
        const string markdown = """
        ## Step 1

        Do the thing.
        """;

        var html = TutorialContentFormatter.BuildLegacyHtmlFromMarkdown(markdown);

        Assert.Contains("<h2>Step 1</h2>", html);
        Assert.Contains("<p>Do the thing.</p>", html);
    }
}
