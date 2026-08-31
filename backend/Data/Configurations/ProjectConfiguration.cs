using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasIndex(p => p.CandidateProfileId);
        builder.HasIndex(p => p.EndDate);  // For sorting recent projects

        builder.Property(p => p.Name).HasMaxLength(300).IsRequired();
        builder.Property(p => p.Description); // Markdown, no length limit

        builder.Property(p => p.RowVersion)
               .IsRowVersion()
               .IsConcurrencyToken();

        builder.HasMany(p => p.Tags)
               .WithOne(t => t.Project)
               .HasForeignKey(t => t.ProjectId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProjectTagConfiguration : IEntityTypeConfiguration<ProjectTag>
{
    public void Configure(EntityTypeBuilder<ProjectTag> builder)
    {
        // Index for autocomplete: "SELECT DISTINCT tag WHERE tag LIKE 'prefix%'"
        builder.HasIndex(t => t.Tag);
        builder.HasIndex(t => new { t.ProjectId, t.Tag }).IsUnique();
        builder.Property(t => t.Tag).HasMaxLength(100).IsRequired();
    }
}
