using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class AttributeDefinitionConfiguration : IEntityTypeConfiguration<AttributeDefinition>
{
    public void Configure(EntityTypeBuilder<AttributeDefinition> builder)
    {
        builder.ToTable("AttributeDefinitions");

        // Globally unique name (Section 8)
        builder.HasIndex(a => a.Name).IsUnique();
        builder.HasIndex(a => a.Category);
        builder.HasIndex(a => a.IsDeleted);

        builder.Property(a => a.Name).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Category).HasMaxLength(100);
        builder.Property(a => a.Description).HasMaxLength(2000);

        builder.HasMany(a => a.Options)
               .WithOne(o => o.AttributeDefinition)
               .HasForeignKey(o => o.AttributeDefinitionId)
               .OnDelete(DeleteBehavior.Cascade);

        // Global query filter to exclude soft-deleted attributes
        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
