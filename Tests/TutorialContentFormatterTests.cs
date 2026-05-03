using InsTK.Server.Components.Pages.Tutorials;
using InsTK.Shared.Models.Tutorials;
using Xunit;

namespace Tests;

public sealed class TutorialContentFormatterTests
{
    [Fact]
    public void BuildWordPressHtml_UsesContentMarkdown()
    {
        var tutorial = new TutorialDefinition
        {
            Title = "Sample Tutorial",
            ContentMarkdown = "## Step 1\n\nDo the thing."
        };

        var html = TutorialContentFormatter.BuildWordPressHtml(tutorial);

        Assert.Contains("<h2>Step 1</h2>", html);
        Assert.Contains("<p>Do the thing.</p>", html);
    }

    [Fact]
    public void BuildWordPressMarkdown_UsesContentMarkdown()
    {
        var tutorial = new TutorialDefinition
        {
            Title = "Sample Tutorial",
            ContentMarkdown = "## Step 1\n\nDo the thing."
        };

        var markdown = TutorialContentFormatter.BuildWordPressMarkdown(tutorial);

        Assert.Contains("## Step 1", markdown);
        Assert.Contains("Do the thing.", markdown);
    }

    [Fact]
    public void BuildBrightspaceAssignmentHtml_UsesAssignmentFieldsOnly()
    {
        var tutorial = new TutorialDefinition
        {
            Title = "Sample Tutorial",
            ContentMarkdown = "This should never appear in Brightspace export.",
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
    }

    [Fact]
    public void RenderMarkdown_StripsUnsafeScriptAndEventHandlerMarkup()
    {
        const string markdown = """
        <script>alert('x')</script>
        <p onclick="alert('x')">Safe text</p>
        """;

        var html = TutorialContentFormatter.RenderMarkdown(markdown);

        Assert.DoesNotContain("<script", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onclick", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<p>Safe text</p>", html);
    }

    [Fact]
    public void RenderMarkdown_StripsJavascriptUrlsButPreservesSafeLinksAndImages()
    {
        const string markdown = """
        [Good](https://example.com/docs)
        <a href="javascript:alert('x')">Bad</a>
        <img src="/uploads/example.png" alt="sample" onerror="alert('x')" />
        """;

        var html = TutorialContentFormatter.RenderMarkdown(markdown);

        Assert.Contains("href=\"https://example.com/docs\"", html);
        Assert.DoesNotContain("javascript:alert", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("src=\"/uploads/example.png\"", html);
        Assert.DoesNotContain("onerror", html, StringComparison.OrdinalIgnoreCase);
    }
}
