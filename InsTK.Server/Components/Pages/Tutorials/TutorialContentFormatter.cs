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

            foreach (var step in tutorial.Steps.OrderBy(s => s.DisplayOrder))
            {
                html.AppendLine($"<h2>Step {step.DisplayOrder}: {Encode(step.Title)}</h2>");
                html.AppendLine(RenderMarkdown(step.InstructionMarkdown));
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

            foreach (var step in tutorial.Steps.OrderBy(s => s.DisplayOrder))
            {
                markdown.AppendLine($"## Step {step.DisplayOrder}: {step.Title}");
                markdown.AppendLine();
                markdown.AppendLine((step.InstructionMarkdown ?? string.Empty).Trim());
                markdown.AppendLine();
            }

            return markdown.ToString().Trim();
        }
        private static string Encode(string? value)
        {
            return WebUtility.HtmlEncode(value ?? string.Empty);
        }
    }
}
