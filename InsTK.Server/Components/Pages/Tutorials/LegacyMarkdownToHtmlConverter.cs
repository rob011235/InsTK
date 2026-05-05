using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace InsTK.Server.Components.Pages.Tutorials
{
    internal static partial class LegacyMarkdownToHtmlConverter
    {
        [GeneratedRegex(@"!\[(?<alt>[^\]]*)\]\((?<url>[^)]+)\)", RegexOptions.Compiled)]
        private static partial Regex MarkdownImageRegex();

        [GeneratedRegex(@"\[(?<text>[^\]]+)\]\((?<url>[^)]+)\)", RegexOptions.Compiled)]
        private static partial Regex MarkdownLinkRegex();

        [GeneratedRegex(@"`(?<code>[^`]+)`", RegexOptions.Compiled)]
        private static partial Regex InlineCodeRegex();

        [GeneratedRegex(@"\*\*(?<text>.+?)\*\*", RegexOptions.Compiled)]
        private static partial Regex BoldRegex();

        [GeneratedRegex(@"\*(?<text>.+?)\*", RegexOptions.Compiled)]
        private static partial Regex ItalicRegex();

        public static string Convert(string? markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
            {
                return string.Empty;
            }

            var lines = markdown.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
            var html = new StringBuilder();
            var paragraphLines = new List<string>();
            var listItems = new List<string>();
            var listTag = string.Empty;
            var codeBlockLines = new List<string>();
            var insideCodeBlock = false;

            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd();
                var trimmed = line.Trim();

                if (trimmed.StartsWith("```", StringComparison.Ordinal))
                {
                    FlushParagraph(html, paragraphLines);
                    FlushList(html, listItems, ref listTag);

                    if (insideCodeBlock)
                    {
                        html.Append("<pre><code>")
                            .Append(WebUtility.HtmlEncode(string.Join("\n", codeBlockLines)))
                            .AppendLine("</code></pre>");
                        codeBlockLines.Clear();
                        insideCodeBlock = false;
                    }
                    else
                    {
                        insideCodeBlock = true;
                    }

                    continue;
                }

                if (insideCodeBlock)
                {
                    codeBlockLines.Add(line);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(trimmed))
                {
                    FlushParagraph(html, paragraphLines);
                    FlushList(html, listItems, ref listTag);
                    continue;
                }

                var headingLevel = CountHeadingLevel(trimmed);
                if (headingLevel > 0)
                {
                    FlushParagraph(html, paragraphLines);
                    FlushList(html, listItems, ref listTag);

                    var headingText = trimmed[(headingLevel + 1)..];
                    html.Append('<').Append('h').Append(headingLevel).Append('>')
                        .Append(TransformInline(headingText))
                        .Append("</h").Append(headingLevel).AppendLine(">");
                    continue;
                }

                if (TryGetListItem(trimmed, out var detectedListTag, out var listItem))
                {
                    FlushParagraph(html, paragraphLines);

                    if (!string.Equals(listTag, detectedListTag, StringComparison.Ordinal))
                    {
                        FlushList(html, listItems, ref listTag);
                        listTag = detectedListTag;
                    }

                    listItems.Add(listItem);
                    continue;
                }

                if (trimmed.StartsWith(">", StringComparison.Ordinal))
                {
                    FlushParagraph(html, paragraphLines);
                    FlushList(html, listItems, ref listTag);

                    var quoteText = trimmed.TrimStart('>').TrimStart();
                    html.Append("<blockquote><p>")
                        .Append(TransformInline(quoteText))
                        .AppendLine("</p></blockquote>");
                    continue;
                }

                paragraphLines.Add(trimmed);
            }

            FlushParagraph(html, paragraphLines);
            FlushList(html, listItems, ref listTag);

            if (insideCodeBlock && codeBlockLines.Count > 0)
            {
                html.Append("<pre><code>")
                    .Append(WebUtility.HtmlEncode(string.Join("\n", codeBlockLines)))
                    .AppendLine("</code></pre>");
            }

            return html.ToString().Trim();
        }

        private static int CountHeadingLevel(string line)
        {
            var level = 0;
            while (level < line.Length && line[level] == '#')
            {
                level++;
            }

            return level is >= 1 and <= 6 &&
                   line.Length > level &&
                   line[level] == ' '
                ? level
                : 0;
        }

        private static bool TryGetListItem(string line, out string listTag, out string listItem)
        {
            listTag = string.Empty;
            listItem = string.Empty;

            if (line.StartsWith("- ", StringComparison.Ordinal) ||
                line.StartsWith("* ", StringComparison.Ordinal) ||
                line.StartsWith("+ ", StringComparison.Ordinal))
            {
                listTag = "ul";
                listItem = TransformInline(line[2..]);
                return true;
            }

            var dotIndex = line.IndexOf(". ", StringComparison.Ordinal);
            if (dotIndex > 0 &&
                int.TryParse(line[..dotIndex], out _))
            {
                listTag = "ol";
                listItem = TransformInline(line[(dotIndex + 2)..]);
                return true;
            }

            return false;
        }

        private static void FlushParagraph(StringBuilder html, List<string> paragraphLines)
        {
            if (paragraphLines.Count == 0)
            {
                return;
            }

            var content = string.Join("<br />", paragraphLines.Select(TransformInline));
            html.Append("<p>").Append(content).AppendLine("</p>");
            paragraphLines.Clear();
        }

        private static void FlushList(StringBuilder html, List<string> listItems, ref string listTag)
        {
            if (listItems.Count == 0 || string.IsNullOrWhiteSpace(listTag))
            {
                listItems.Clear();
                listTag = string.Empty;
                return;
            }

            html.Append('<').Append(listTag).AppendLine(">");
            foreach (var item in listItems)
            {
                html.Append("<li>").Append(item).AppendLine("</li>");
            }

            html.Append("</").Append(listTag).AppendLine(">");
            listItems.Clear();
            listTag = string.Empty;
        }

        private static string TransformInline(string value)
        {
            var encoded = WebUtility.HtmlEncode(value);

            encoded = MarkdownImageRegex().Replace(
                encoded,
                match => BuildImageTag(
                    WebUtility.HtmlDecode(match.Groups["alt"].Value),
                    WebUtility.HtmlDecode(match.Groups["url"].Value)));

            encoded = MarkdownLinkRegex().Replace(
                encoded,
                match => BuildLinkTag(
                    WebUtility.HtmlDecode(match.Groups["text"].Value),
                    WebUtility.HtmlDecode(match.Groups["url"].Value)));

            encoded = InlineCodeRegex().Replace(
                encoded,
                match => $"<code>{match.Groups["code"].Value}</code>");

            encoded = BoldRegex().Replace(
                encoded,
                match => $"<strong>{match.Groups["text"].Value}</strong>");

            encoded = ItalicRegex().Replace(
                encoded,
                match => $"<em>{match.Groups["text"].Value}</em>");

            return encoded;
        }

        private static string BuildImageTag(string altText, string url)
        {
            if (!TutorialContentFormatter.TrySanitizeUrl(url, out var sanitizedUrl))
            {
                return WebUtility.HtmlEncode($"![{altText}]({url})");
            }

            return $"<img src=\"{WebUtility.HtmlEncode(sanitizedUrl)}\" alt=\"{WebUtility.HtmlEncode(altText)}\" />";
        }

        private static string BuildLinkTag(string text, string url)
        {
            if (!TutorialContentFormatter.TrySanitizeUrl(url, out var sanitizedUrl))
            {
                return WebUtility.HtmlEncode($"[{text}]({url})");
            }

            var builder = new StringBuilder()
                .Append("<a href=\"")
                .Append(WebUtility.HtmlEncode(sanitizedUrl))
                .Append('"');

            if (sanitizedUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                sanitizedUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                builder.Append(" target=\"_blank\"");
            }

            builder.Append('>')
                .Append(WebUtility.HtmlEncode(text))
                .Append("</a>");

            return builder.ToString();
        }
    }
}
