using System.ComponentModel.DataAnnotations;

namespace InsTK.Shared.Models.Tutorials
{
    public class TutorialDefinition
    {
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

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedUtc { get; set; }

        public List<TutorialStep> Steps { get; set; } = new();
    }
}
