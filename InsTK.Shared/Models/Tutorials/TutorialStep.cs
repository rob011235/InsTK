using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Instruction { get; set; } = string.Empty;

        public string CodeSnippet { get; set; } = string.Empty;
        public string CodeLanguage { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;
        public string ImageAltText { get; set; } = string.Empty;

        public string Note { get; set; } = string.Empty;

        public List<EvidenceHint> EvidenceHints { get; set; } = new();
    }
}
