using InsTK.Shared.Models.Tutorials;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InsTK.Server.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<TutorialDefinition> Tutorials => Set<TutorialDefinition>();
        public DbSet<TutorialStep> TutorialSteps => Set<TutorialStep>();
        public DbSet<EvidenceHint> EvidenceHints => Set<EvidenceHint>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<TutorialDefinition>()
                .HasMany(t => t.Steps)
                .WithOne(s => s.TutorialDefinition)
                .HasForeignKey(s => s.TutorialDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TutorialStep>()
                .HasMany(s => s.EvidenceHints)
                .WithOne(e => e.TutorialStep)
                .HasForeignKey(e => e.TutorialStepId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
