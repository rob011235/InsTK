using System.ComponentModel.DataAnnotations;

namespace InsTK.Shared.Models.Tutorials
{
    public class TutorialDefinition
    {
        public const string DefaultBrightspaceSubmissionInstructions = """
        Submit the URL to your GitHub repository in the Brightspace submission comments.

        The first URL in your submission will be used for grading.

        If you are not using the main branch, you must include the branch in the URL using this format:

        https://github.com/{owner}/{repository}/tree/{branch}

        Examples:

        https://github.com/jdoe/BlazorApp
        https://github.com/cnm-students/BlazorApp/tree/feature/tutorial-step-3

        You can copy the correct URL directly from GitHub by selecting your branch and copying the page URL.

        Only submit a repository URL. Do not submit links to files, pull requests, or other pages.

        Submissions that do not follow this format may not be graded correctly.
        """;

        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Summary { get; set; }

        [StringLength(100)]
        public string? Technology { get; set; }

        public string? IntroMarkdown { get; set; }

        public string? ConclusionMarkdown { get; set; }

        [Url]
        [StringLength(500)]
        public string? RepoUrl { get; set; }

        [StringLength(200)]
        public string? BranchName { get; set; }

        [StringLength(300)]
        public string? ReferenceSubPath { get; set; }

        public string? ReferenceFileNames { get; set; }

        public string? ReferenceCode { get; set; }

        public string? GradingHints { get; set; }

        [StringLength(200)]
        public string? BrightspaceAssignmentTitle { get; set; }

        public string? BrightspaceAssignmentInstructions { get; set; }

        public int BrightspacePoints { get; set; } = 100;

        [StringLength(500)]
        public string? BrightspaceSubmissionInstructions { get; set; } = DefaultBrightspaceSubmissionInstructions;

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedUtc { get; set; }

        public List<TutorialStep> Steps { get; set; } = new();
    }
}
