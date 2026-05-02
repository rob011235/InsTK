using System.Net;
using System.Text;
using InsTK.Shared.Models.Tutorials;
using Markdig;

namespace InsTK.Server.Components.Pages.Tutorials
{
    internal static class TutorialContentFormatter
    {
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
                ? tutorial.Title
                : tutorial.BrightspaceAssignmentTitle;

            html.AppendLine($"<h1>{Encode(title)}</h1>");

            html.AppendLine("<h2>Assignment Overview</h2>");

            if (!string.IsNullOrWhiteSpace(tutorial.BrightspaceAssignmentInstructions))
            {
                html.AppendLine(RenderMarkdown(tutorial.BrightspaceAssignmentInstructions));
            }
            else if (!string.IsNullOrWhiteSpace(tutorial.Summary))
            {
                html.AppendLine($"<p>{Encode(tutorial.Summary)}</p>");
            }

            html.AppendLine("<h2>Tutorial Instructions</h2>");
            html.AppendLine(BuildWordPressHtml(tutorial));

            html.AppendLine("<h2>Submission Requirements</h2>");

            if (!string.IsNullOrWhiteSpace(tutorial.BrightspaceSubmissionInstructions))
            {
                html.AppendLine(RenderMarkdown(tutorial.BrightspaceSubmissionInstructions));
            }
            else
            {
                html.AppendLine("""
        <p>Submit the URL to your GitHub repository in the Brightspace submission comments.</p>
        <p>If your work is not on the main branch, include the branch name.</p>
        """);
            }

            html.AppendLine("<h2>Points</h2>");
            html.AppendLine($"<p>This assignment is worth {tutorial.BrightspacePoints} points.</p>");

            return html.ToString().Trim();
        }
    }
}
