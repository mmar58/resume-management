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
