using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsTK.Shared.Models.Tutorials
{
    public class EvidenceHint
    {
        public int Id { get; set; }

        public int TutorialStepId { get; set; }
        public TutorialStep? TutorialStep { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Value { get; set; } = string.Empty;

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
    }
}