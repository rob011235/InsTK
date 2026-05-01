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
        public string Title { get; set; } = string.Empty;

        [Required]
        public string InstructionMarkdown { get; set; } = string.Empty;
    }
}
