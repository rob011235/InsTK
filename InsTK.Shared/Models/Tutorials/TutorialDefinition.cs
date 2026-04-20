using System.ComponentModel.DataAnnotations;

namespace InsTK.Shared.Models.Tutorials
{
    public class TutorialDefinition
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        [StringLength(500)]
        public string Summary { get; set; } = string.Empty;

        [StringLength(100)]
        public string Technology { get; set; } = string.Empty;

        [StringLength(200)]
        public string Tags { get; set; } = string.Empty;

        [StringLength(200)]
        public string Slug { get; set; } = string.Empty;

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedUtc { get; set; }

        public List<TutorialStep> Steps { get; set; } = new();
    }
}