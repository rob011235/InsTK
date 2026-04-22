using System.ComponentModel.DataAnnotations;

namespace InsTK.Shared.Models.Tutorials
{
    public class TutorialStep
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TutorialDefinitionId { get; set; }
        public TutorialDefinition? TutorialDefinition { get; set; }

        public int DisplayOrder { get; set; }

        [Required]
        [StringLength(200)]
        public string Heading { get; set; } = string.Empty;

        [Required]
        public string MarkdownContent { get; set; } = string.Empty;

        public List<EvidenceHint> EvidenceHints { get; set; } = new();
    }
}