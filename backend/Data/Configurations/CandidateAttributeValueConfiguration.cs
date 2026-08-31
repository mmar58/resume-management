using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class CandidateAttributeValueConfiguration : IEntityTypeConfiguration<CandidateAttributeValue>
{
    public void Configure(EntityTypeBuilder<CandidateAttributeValue> builder)
    {
        // A candidate has at most one value per attribute
        builder.HasIndex(v => new { v.CandidateProfileId, v.AttributeDefinitionId }).IsUnique();
        builder.HasIndex(v => v.AttributeDefinitionId);

        builder.Property(v => v.StringValue).HasMaxLength(1000);
        builder.Property(v => v.ImageUrl).HasMaxLength(1000);
        builder.Property(v => v.OptionValue).HasMaxLength(500);
        builder.Property(v => v.NumericValue).HasPrecision(18, 4);

        builder.Property(v => v.RowVersion)
               .IsRowVersion()
               .IsConcurrencyToken();

        builder.HasOne(v => v.CandidateProfile)
               .WithMany(p => p.AttributeValues)
               .HasForeignKey(v => v.CandidateProfileId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.AttributeDefinition)
               .WithMany(a => a.CandidateValues)
               .HasForeignKey(v => v.AttributeDefinitionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
