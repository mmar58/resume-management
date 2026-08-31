using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class CVConfiguration : IEntityTypeConfiguration<CV>
{
    public void Configure(EntityTypeBuilder<CV> builder)
    {
        // Enforce: one CV per (Candidate, Position) (Section 16, rule 16)
        builder.HasIndex(cv => new { cv.CandidateProfileId, cv.PositionId }).IsUnique();
        builder.HasIndex(cv => cv.PositionId);
        builder.HasIndex(cv => cv.Status);
        builder.HasIndex(cv => cv.CreatedAt);

        builder.Property(cv => cv.RowVersion)
               .IsRowVersion()
               .IsConcurrencyToken();

        builder.HasMany(cv => cv.Likes)
               .WithOne(l => l.CV)
               .HasForeignKey(l => l.CVId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(cv => cv.SelectedAttributes)
               .WithOne(sa => sa.CV)
               .HasForeignKey(sa => sa.CVId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(cv => cv.SelectedProjects)
               .WithOne(sp => sp.CV)
               .HasForeignKey(sp => sp.CVId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CVAttributeValueConfiguration : IEntityTypeConfiguration<CVAttributeValue>
{
    public void Configure(EntityTypeBuilder<CVAttributeValue> builder)
    {
        builder.HasIndex(a => new { a.CVId, a.CandidateAttributeValueId }).IsUnique();
        
        builder.HasOne(a => a.CandidateAttributeValue)
               .WithMany()
               .HasForeignKey(a => a.CandidateAttributeValueId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class CVProjectConfiguration : IEntityTypeConfiguration<CVProject>
{
    public void Configure(EntityTypeBuilder<CVProject> builder)
    {
        builder.HasIndex(p => new { p.CVId, p.ProjectId }).IsUnique();
        
        builder.HasOne(p => p.Project)
               .WithMany()
               .HasForeignKey(p => p.ProjectId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class CVLikeConfiguration : IEntityTypeConfiguration<CVLike>
{
    public void Configure(EntityTypeBuilder<CVLike> builder)
    {
        // Enforce: one like per (CV, Recruiter) at DB level (Section 19)
        builder.HasIndex(l => new { l.CVId, l.RecruiterId }).IsUnique();
    }
}
