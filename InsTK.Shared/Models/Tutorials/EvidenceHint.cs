using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsTK.Shared.Models.Tutorials
{
    public class EvidenceHint
    {
        public int Id { get; set; }

        public int TutorialStepId { get; set; }
        public TutorialStep? TutorialStep { get; set; }

        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}