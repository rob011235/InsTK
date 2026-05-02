using System.Net;
using System.Text;
using InsTK.Shared.Models.Tutorials;
using Markdig;

namespace InsTK.Server.Components.Pages.Tutorials
{
    internal static class TutorialContentFormatter
    {
        private const string DefaultBrightspaceSubmissionInstructions = """
        <p>Submit the URL to your GitHub repository in the Brightspace submission comments.</p>

        <p>The first URL in your submission will be used for grading.</p>

        <p>If you are not using the main branch, you must include the branch in the URL using this format:</p>

        <pre><code>https://github.com/{owner}/{repository}/tree/{branch}</code></pre>

        <p>Examples:</p>

        <pre><code>https://github.com/jdoe/BlazorApp
        https://github.com/cnm-students/BlazorApp/tree/feature/tutorial-step-3</code></pre>

        <p>You can copy the correct URL directly from GitHub by selecting your branch and copying the page URL.</p>

        <p>Only submit a repository URL. Do not submit links to files, pull requests, or other pages.</p>

        <p>Submissions that do not follow this format may not be graded correctly.</p>
        """;

        private static readonly MarkdownPipeline MarkdownPipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

        public static string RenderMarkdown(string? markdown)
        {
            return Markdown.ToHtml(markdown ?? string.Empty, MarkdownPipeline).Trim();
        }

        public static string BuildWordPressHtml(TutorialDefinition tutorial)
        {
            var html = new StringBuilder();

            html.AppendLine($"<h1>{Encode(tutorial.Title)}</h1>");

            if (!string.IsNullOrWhiteSpace(tutorial.Summary))
            {
                html.AppendLine($"<p>{Encode(tutorial.Summary)}</p>");
            }

            if (!string.IsNullOrWhiteSpace(tutorial.Technology))
            {
                html.AppendLine($"<p><strong>Technology:</strong> {Encode(tutorial.Technology)}</p>");
            }

            if (!string.IsNullOrWhiteSpace(tutorial.IntroMarkdown))
            {
                html.AppendLine(RenderMarkdown(tutorial.IntroMarkdown));
            }

            foreach (var step in tutorial.Steps.OrderBy(s => s.DisplayOrder))
            {
                html.AppendLine($"<h2>Step {step.DisplayOrder}: {Encode(step.Title)}</h2>");
                html.AppendLine(RenderMarkdown(step.InstructionMarkdown));
            }

            if (!string.IsNullOrWhiteSpace(tutorial.ConclusionMarkdown))
            {
                html.AppendLine(RenderMarkdown(tutorial.ConclusionMarkdown));
            }

            return html.ToString().Trim();
        }

        public static string BuildWordPressMarkdown(TutorialDefinition tutorial)
        {
            var markdown = new StringBuilder();

            markdown.AppendLine($"# {tutorial.Title}");
            markdown.AppendLine();

            if (!string.IsNullOrWhiteSpace(tutorial.Summary))
            {
                markdown.AppendLine(tutorial.Summary.Trim());
                markdown.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(tutorial.Technology))
            {
                markdown.AppendLine($"Technology: {tutorial.Technology.Trim()}");
                markdown.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(tutorial.IntroMarkdown))
            {
                markdown.AppendLine(tutorial.IntroMarkdown.Trim());
                markdown.AppendLine();
            }

            foreach (var step in tutorial.Steps.OrderBy(s => s.DisplayOrder))
            {
                markdown.AppendLine($"## Step {step.DisplayOrder}: {step.Title}");
                markdown.AppendLine();
                markdown.AppendLine((step.InstructionMarkdown ?? string.Empty).Trim());
                markdown.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(tutorial.ConclusionMarkdown))
            {
                markdown.AppendLine("## Conclusion");
                markdown.AppendLine();
                markdown.AppendLine(tutorial.ConclusionMarkdown.Trim());
                markdown.AppendLine();
            }

            return markdown.ToString().Trim();
        }

        private static string Encode(string? value)
        {
            return WebUtility.HtmlEncode(value ?? string.Empty);
        }

        public static string BuildBrightspaceAssignmentHtml(TutorialDefinition tutorial)
        {
            var html = new StringBuilder();

            var title = string.IsNullOrWhiteSpace(tutorial.BrightspaceAssignmentTitle)
                ? BuildDefaultAssignmentTitle(tutorial.Title)
                : tutorial.BrightspaceAssignmentTitle.Trim();

            html.AppendLine($"<h1>{Encode(title)}</h1>");

            html.AppendLine("<h2>Assignment Overview</h2>");

            if (!string.IsNullOrWhiteSpace(tutorial.BrightspaceAssignmentInstructions))
            {
                var instructions = tutorial.BrightspaceAssignmentInstructions
                    .Replace("{{url}}", BuildTutorialUrl(tutorial));

                html.AppendLine(RenderMarkdown(instructions));
            }
            else if (!string.IsNullOrWhiteSpace(tutorial.Summary))
            {
                html.AppendLine($"<p>{Encode(tutorial.Summary)}</p>");
            }

            html.AppendLine("<h2>Tutorial Instructions</h2>");

            if (!string.IsNullOrWhiteSpace(tutorial.IntroMarkdown))
            {
                html.AppendLine(RenderMarkdown(tutorial.IntroMarkdown));
            }

            foreach (var step in tutorial.Steps.OrderBy(s => s.DisplayOrder))
            {
                html.AppendLine($"<h3>Step {step.DisplayOrder}: {Encode(step.Title)}</h3>");
                html.AppendLine(RenderMarkdown(step.InstructionMarkdown));
            }

            if (!string.IsNullOrWhiteSpace(tutorial.ConclusionMarkdown))
            {
                html.AppendLine("<h3>Conclusion</h3>");
                html.AppendLine(RenderMarkdown(tutorial.ConclusionMarkdown));
            }

            html.AppendLine("<h2>Submission Requirements</h2>");

            if (!string.IsNullOrWhiteSpace(tutorial.BrightspaceSubmissionInstructions))
            {
                html.AppendLine(RenderMarkdown(tutorial.BrightspaceSubmissionInstructions));
            }

            html.AppendLine("<h2>Points</h2>");
            html.AppendLine($"<p>This assignment is worth {tutorial.BrightspacePoints} points.</p>");

            return html.ToString().Trim();
        }

        private static string BuildDefaultAssignmentTitle(string? tutorialTitle)
        {
            return string.IsNullOrWhiteSpace(tutorialTitle)
                ? "Tutorial"
                : $"Tutorial - {tutorialTitle.Trim()}";
        }

        private static string BuildTutorialUrl(TutorialDefinition tutorial)
        {
            // Adjust this to match your actual route
            return $"https:///robgarnerblog.wordpress.com/tutorials/{tutorial.Title?.ToLower().Replace(' ', '-')}";
        }
    }
}
