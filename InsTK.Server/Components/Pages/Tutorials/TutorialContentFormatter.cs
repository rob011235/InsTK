using System.Net;
using System.Text;
using InsTK.Shared.Models.Tutorials;

namespace InsTK.Server.Components.Pages.Tutorials
{
    internal static class TutorialContentFormatter
    {
        public static string RenderMarkdown(string? markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
            {
                return string.Empty;
            }

            var lines = markdown.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
            var html = new StringBuilder();
            var paragraphLines = new List<string>();
            var inUnorderedList = false;
            var inOrderedList = false;
            var inCodeBlock = false;

            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd();
                var trimmed = line.Trim();

                if (trimmed.StartsWith("```", StringComparison.Ordinal))
                {
                    FlushParagraph(html, paragraphLines);
                    CloseLists(html, ref inUnorderedList, ref inOrderedList);

                    if (!inCodeBlock)
                    {
                        html.AppendLine("<pre><code>");
                    }
                    else
                    {
                        html.AppendLine("</code></pre>");
                    }

                    inCodeBlock = !inCodeBlock;
                    continue;
                }

                if (inCodeBlock)
                {
                    html.AppendLine(WebUtility.HtmlEncode(rawLine));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(trimmed))
                {
                    FlushParagraph(html, paragraphLines);
                    CloseLists(html, ref inUnorderedList, ref inOrderedList);
                    continue;
                }

                var isHeading = IsHeadingLine(trimmed);
                var isUnorderedListItem = IsUnorderedListItem(trimmed);
                var isOrderedListItem = GetOrderedListMarkerLength(trimmed) > 0;

                if (isHeading || isUnorderedListItem || isOrderedListItem)
                {
                    FlushParagraph(html, paragraphLines);
                }

                if (isHeading && TryRenderHeading(trimmed, html))
                {
                    continue;
                }

                if (isUnorderedListItem && TryRenderUnorderedListItem(trimmed, html, ref inUnorderedList, ref inOrderedList))
                {
                    continue;
                }

                if (isOrderedListItem && TryRenderOrderedListItem(trimmed, html, ref inOrderedList, ref inUnorderedList))
                {
                    continue;
                }

                paragraphLines.Add(trimmed);
            }

            FlushParagraph(html, paragraphLines);
            CloseLists(html, ref inUnorderedList, ref inOrderedList);

            if (inCodeBlock)
            {
                html.AppendLine("</code></pre>");
            }

            return html.ToString().Trim();
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

        private static void FlushParagraph(StringBuilder html, List<string> paragraphLines)
        {
            if (paragraphLines.Count == 0)
            {
                return;
            }

            html.Append("<p>");
            html.Append(string.Join(" ", paragraphLines.Select(FormatInlineText)));
            html.AppendLine("</p>");
            paragraphLines.Clear();
        }

        private static void CloseLists(StringBuilder html, ref bool inUnorderedList, ref bool inOrderedList)
        {
            if (inUnorderedList)
            {
                html.AppendLine("</ul>");
                inUnorderedList = false;
            }

            if (inOrderedList)
            {
                html.AppendLine("</ol>");
                inOrderedList = false;
            }
        }

        private static bool TryRenderHeading(string line, StringBuilder html)
        {
            if (!IsHeadingLine(line))
            {
                return false;
            }

            var level = 0;

            while (level < line.Length && line[level] == '#')
            {
                level++;
            }

            if (level == 0 || level > 6 || level >= line.Length || line[level] != ' ')
            {
                return false;
            }

            html.AppendLine($"<h{level}>{FormatInlineText(line[(level + 1)..])}</h{level}>");
            return true;
        }

        private static bool TryRenderUnorderedListItem(string line, StringBuilder html, ref bool inUnorderedList, ref bool inOrderedList)
        {
            if (!IsUnorderedListItem(line))
            {
                return false;
            }

            if (inOrderedList)
            {
                html.AppendLine("</ol>");
                inOrderedList = false;
            }

            if (!inUnorderedList)
            {
                html.AppendLine("<ul>");
                inUnorderedList = true;
            }

            html.AppendLine($"<li>{FormatInlineText(line[2..])}</li>");
            return true;
        }

        private static bool TryRenderOrderedListItem(string line, StringBuilder html, ref bool inOrderedList, ref bool inUnorderedList)
        {
            var markerLength = GetOrderedListMarkerLength(line);
            if (markerLength == 0)
            {
                return false;
            }

            if (inUnorderedList)
            {
                html.AppendLine("</ul>");
                inUnorderedList = false;
            }

            if (!inOrderedList)
            {
                html.AppendLine("<ol>");
                inOrderedList = true;
            }

            html.AppendLine($"<li>{FormatInlineText(line[markerLength..])}</li>");
            return true;
        }

        private static int GetOrderedListMarkerLength(string line)
        {
            var index = 0;

            while (index < line.Length && char.IsDigit(line[index]))
            {
                index++;
            }

            if (index == 0 || index + 1 >= line.Length || line[index] != '.' || line[index + 1] != ' ')
            {
                return 0;
            }

            return index + 2;
        }

        private static bool IsHeadingLine(string line)
        {
            return line.StartsWith('#');
        }

        private static bool IsUnorderedListItem(string line)
        {
            return line.StartsWith("- ", StringComparison.Ordinal) || line.StartsWith("* ", StringComparison.Ordinal);
        }

        private static string FormatInlineText(string value)
        {
            var html = new StringBuilder();
            var inCode = false;

            foreach (var ch in value.Trim())
            {
                if (ch == '`')
                {
                    html.Append(inCode ? "</code>" : "<code>");
                    inCode = !inCode;
                    continue;
                }

                html.Append(WebUtility.HtmlEncode(ch.ToString()));
            }

            if (inCode)
            {
                html.Append("</code>");
            }

            return html.ToString();
        }

        private static string Encode(string? value)
        {
            return WebUtility.HtmlEncode(value ?? string.Empty);
        }
    }
}
