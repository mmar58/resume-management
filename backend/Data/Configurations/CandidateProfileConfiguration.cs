using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class CandidateProfileConfiguration : IEntityTypeConfiguration<CandidateProfile>
{
    public void Configure(EntityTypeBuilder<CandidateProfile> builder)
    {
        builder.HasIndex(p => p.UserId).IsUnique();

        builder.Property(p => p.FirstName).HasMaxLength(100);
        builder.Property(p => p.LastName).HasMaxLength(100);
        builder.Property(p => p.Location).HasMaxLength(200);
        builder.Property(p => p.PhotoUrl).HasMaxLength(1000);

        builder.Property(p => p.RowVersion)
               .IsRowVersion()
               .IsConcurrencyToken();

        builder.HasMany(p => p.AttributeValues)
               .WithOne(v => v.CandidateProfile)
               .HasForeignKey(v => v.CandidateProfileId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Projects)
               .WithOne(pr => pr.CandidateProfile)
               .HasForeignKey(pr => pr.CandidateProfileId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.CVs)
               .WithOne(cv => cv.CandidateProfile)
               .HasForeignKey(cv => cv.CandidateProfileId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
