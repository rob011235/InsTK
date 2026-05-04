using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using InsTK.Shared.Models.Tutorials;
using Markdig;

namespace InsTK.Server.Components.Pages.Tutorials
{
    internal static class TutorialContentFormatter
    {
        private static readonly HashSet<string> AllowedElements =
        [
            "a",
            "blockquote",
            "br",
            "code",
            "del",
            "em",
            "h1",
            "h2",
            "h3",
            "h4",
            "h5",
            "h6",
            "hr",
            "img",
            "li",
            "ol",
            "p",
            "pre",
            "strong",
            "table",
            "tbody",
            "td",
            "th",
            "thead",
            "tr",
            "ul"
        ];

        private static readonly HashSet<string> StripContentElements =
        [
            "script",
            "style",
            "iframe",
            "object",
            "embed",
            "form",
            "input",
            "button",
            "select",
            "textarea"
        ];

        private static readonly MarkdownPipeline MarkdownPipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

        public static string RenderMarkdown(string? markdown)
        {
            var html = Markdown.ToHtml(markdown ?? string.Empty, MarkdownPipeline);
            return SanitizeHtml(html).Trim();
        }

        public static string BuildWordPressHtml(TutorialDefinition tutorial)
        {
            var html = new StringBuilder();

            html.AppendLine($"<h1>{Encode(tutorial.Title)}</h1>");

            if (!string.IsNullOrWhiteSpace(tutorial.Summary))
            {
                html.AppendLine($"<p>{Encode(tutorial.Summary)}</p>");
            }

            var embedUrl = BuildYouTubeEmbedUrl(tutorial.YouTubeUrl);
            if (embedUrl != null)
            {
                html.AppendLine("""<div class="tutorial-video">""");
                html.AppendLine($"""<iframe width="560" height="315" src="{Encode(embedUrl)}" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" referrerpolicy="strict-origin-when-cross-origin" allowfullscreen></iframe>""");
                html.AppendLine("</div>");
            }

            if (!string.IsNullOrWhiteSpace(tutorial.ContentMarkdown))
            {
                html.AppendLine(RenderMarkdown(tutorial.ContentMarkdown));
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

            var watchUrl = BuildYouTubeWatchUrl(tutorial.YouTubeUrl);
            if (watchUrl != null)
            {
                markdown.AppendLine(watchUrl);
                markdown.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(tutorial.ContentMarkdown))
            {
                markdown.AppendLine(tutorial.ContentMarkdown.Trim());
                markdown.AppendLine();
            }

            return markdown.ToString().Trim();
        }

        private static string Encode(string? value)
        {
            return WebUtility.HtmlEncode(value ?? string.Empty);
        }

        private static string? BuildYouTubeEmbedUrl(string? youtubeUrl)
        {
            var videoId = TryExtractYouTubeVideoId(youtubeUrl);
            return videoId == null ? null : $"https://www.youtube.com/embed/{videoId}";
        }

        private static string? BuildYouTubeWatchUrl(string? youtubeUrl)
        {
            var videoId = TryExtractYouTubeVideoId(youtubeUrl);
            return videoId == null ? null : $"https://www.youtube.com/watch?v={videoId}";
        }

        public static string BuildBrightspaceAssignmentHtml(TutorialDefinition tutorial)
        {
            var html = new StringBuilder();

            var title = string.IsNullOrWhiteSpace(tutorial.BrightspaceAssignmentTitle)
                ? BuildDefaultAssignmentTitle(tutorial.Title)
                : tutorial.BrightspaceAssignmentTitle.Trim();

            html.AppendLine($"<h1>{Encode(title)}</h1>");

            var assignmentInstructions = string.IsNullOrWhiteSpace(tutorial.BrightspaceAssignmentInstructions)
                ? TutorialDefinition.DefaultBrightspaceAssignmentInstructions
                : tutorial.BrightspaceAssignmentInstructions;

            assignmentInstructions = assignmentInstructions.Replace("{{url}}", BuildTutorialUrl(tutorial));

            html.AppendLine("<h2>Assignment Overview</h2>");
            html.AppendLine(RenderMarkdown(assignmentInstructions));

            html.AppendLine("<h2>Submission Requirements</h2>");

            var submissionInstructions = string.IsNullOrWhiteSpace(tutorial.BrightspaceSubmissionInstructions)
                ? TutorialDefinition.DefaultBrightspaceSubmissionInstructions
                : tutorial.BrightspaceSubmissionInstructions;

            html.AppendLine(RenderMarkdown(submissionInstructions));

            html.AppendLine("<h2>Points</h2>");

            var points = tutorial.BrightspacePoints > 0
                ? tutorial.BrightspacePoints
                : 100;

            html.AppendLine($"<p>This assignment is worth {points} points.</p>");

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
            var slug = BuildTutorialSlug(tutorial.Title);
            return $"https://robgarnerblog.wordpress.com/tutorials/{slug}";
        }

        private static string BuildTutorialSlug(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return "tutorial";
            }

            var builder = new StringBuilder();
            var lastWasHyphen = false;

            foreach (var character in title.Trim().ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(character);
                    lastWasHyphen = false;
                    continue;
                }

                if (!lastWasHyphen)
                {
                    builder.Append('-');
                    lastWasHyphen = true;
                }
            }

            var slug = builder.ToString().Trim('-');
            return string.IsNullOrWhiteSpace(slug) ? "tutorial" : slug;
        }

        private static string? TryExtractYouTubeVideoId(string? youtubeUrl)
        {
            if (string.IsNullOrWhiteSpace(youtubeUrl))
            {
                return null;
            }

            if (!Uri.TryCreate(youtubeUrl.Trim(), UriKind.Absolute, out var uri))
            {
                return null;
            }

            if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var host = uri.Host.Trim().ToLowerInvariant();
            if (host.StartsWith("www.", StringComparison.Ordinal))
            {
                host = host[4..];
            }

            if (host == "youtu.be")
            {
                return NormalizeYouTubeVideoId(uri.AbsolutePath.Trim('/'));
            }

            if (host is not "youtube.com" and not "m.youtube.com")
            {
                return null;
            }

            var path = uri.AbsolutePath.Trim('/');
            if (string.Equals(path, "watch", StringComparison.OrdinalIgnoreCase))
            {
                return NormalizeYouTubeVideoId(ReadQueryParameter(uri.Query, "v"));
            }

            if (path.StartsWith("embed/", StringComparison.OrdinalIgnoreCase))
            {
                return NormalizeYouTubeVideoId(path["embed/".Length..]);
            }

            if (path.StartsWith("shorts/", StringComparison.OrdinalIgnoreCase))
            {
                return NormalizeYouTubeVideoId(path["shorts/".Length..]);
            }

            return null;
        }

        private static string? NormalizeYouTubeVideoId(string? videoId)
        {
            if (string.IsNullOrWhiteSpace(videoId))
            {
                return null;
            }

            var candidate = videoId.Trim();
            var separatorIndex = candidate.IndexOfAny(['/', '?', '&']);
            if (separatorIndex >= 0)
            {
                candidate = candidate[..separatorIndex];
            }

            return candidate.Length == 11 ? candidate : null;
        }

        private static string? ReadQueryParameter(string query, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return null;
            }

            var trimmedQuery = query.TrimStart('?');
            foreach (var segment in trimmedQuery.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = segment.Split('=', 2);
                if (parts.Length == 2 &&
                    string.Equals(parts[0], parameterName, StringComparison.OrdinalIgnoreCase))
                {
                    return Uri.UnescapeDataString(parts[1]);
                }
            }

            return null;
        }

        private static string SanitizeHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            try
            {
                var root = XElement.Parse($"<root>{html}</root>", LoadOptions.PreserveWhitespace);
                var output = new StringBuilder();

                foreach (var node in root.Nodes())
                {
                    AppendSanitizedNode(output, node);
                }

                return output.ToString();
            }
            catch (XmlException)
            {
                return WebUtility.HtmlEncode(html);
            }
        }

        private static void AppendSanitizedNode(StringBuilder output, XNode node)
        {
            switch (node)
            {
                case XText textNode:
                    output.Append(WebUtility.HtmlEncode(textNode.Value));
                    break;

                case XElement element:
                    AppendSanitizedElement(output, element);
                    break;
            }
        }

        private static void AppendSanitizedElement(StringBuilder output, XElement element)
        {
            var tagName = element.Name.LocalName.ToLowerInvariant();

            if (StripContentElements.Contains(tagName))
            {
                return;
            }

            if (!AllowedElements.Contains(tagName))
            {
                foreach (var child in element.Nodes())
                {
                    AppendSanitizedNode(output, child);
                }

                return;
            }

            output.Append('<').Append(tagName);
            AppendAllowedAttributes(output, element, tagName);

            if (IsVoidElement(tagName))
            {
                output.Append(" />");
                return;
            }

            output.Append('>');

            foreach (var child in element.Nodes())
            {
                AppendSanitizedNode(output, child);
            }

            output.Append("</").Append(tagName).Append('>');
        }

        private static void AppendAllowedAttributes(StringBuilder output, XElement element, string tagName)
        {
            foreach (var attribute in element.Attributes())
            {
                var attributeName = attribute.Name.LocalName.ToLowerInvariant();

                if (attributeName.StartsWith("on", StringComparison.Ordinal))
                {
                    continue;
                }

                if (tagName == "a" && attributeName == "href" && TrySanitizeUrl(attribute.Value, out var href))
                {
                    output.Append(' ').Append("href=\"").Append(EncodeAttribute(href)).Append('"');
                    continue;
                }

                if (tagName == "img" && attributeName == "src" && TrySanitizeUrl(attribute.Value, out var src))
                {
                    output.Append(' ').Append("src=\"").Append(EncodeAttribute(src)).Append('"');
                    continue;
                }

                if ((tagName == "a" || tagName == "img") &&
                    (attributeName == "title" || attributeName == "alt"))
                {
                    output.Append(' ').Append(attributeName).Append("=\"")
                        .Append(EncodeAttribute(attribute.Value)).Append('"');
                }
            }
        }

        private static bool TrySanitizeUrl(string? value, out string sanitized)
        {
            sanitized = string.Empty;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var candidate = value.Trim();

            if (candidate.StartsWith("/", StringComparison.Ordinal))
            {
                sanitized = candidate;
                return true;
            }

            if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
            {
                return false;
            }

            if (uri.Scheme != Uri.UriSchemeHttp &&
                uri.Scheme != Uri.UriSchemeHttps &&
                uri.Scheme != Uri.UriSchemeMailto)
            {
                return false;
            }

            sanitized = uri.ToString();
            return true;
        }

        private static bool IsVoidElement(string tagName)
        {
            return tagName is "br" or "hr" or "img";
        }

        private static string EncodeAttribute(string value)
        {
            return WebUtility.HtmlEncode(value);
        }
    }
}
