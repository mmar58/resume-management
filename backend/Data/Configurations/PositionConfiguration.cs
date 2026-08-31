using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.HasIndex(p => p.CreatedAt);   // For "latest positions" query
        builder.HasIndex(p => p.IsActive);

        builder.Property(p => p.Title).HasMaxLength(300).IsRequired();
        builder.Property(p => p.ShortDescription).HasMaxLength(2000);
        builder.Property(p => p.Company).HasMaxLength(200);
        builder.Property(p => p.Level).HasMaxLength(50);

        builder.Property(p => p.RowVersion)
               .IsRowVersion()
               .IsConcurrencyToken();

        builder.HasMany(p => p.PositionAttributes)
               .WithOne(pa => pa.Position)
               .HasForeignKey(pa => pa.PositionId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.ProjectTags)
               .WithOne(pt => pt.Position)
               .HasForeignKey(pt => pt.PositionId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.AccessRules)
               .WithOne(ar => ar.Position)
               .HasForeignKey(ar => ar.PositionId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.CVs)
               .WithOne(cv => cv.Position)
               .HasForeignKey(cv => cv.PositionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.DiscussionPosts)
               .WithOne(dp => dp.Position)
               .HasForeignKey(dp => dp.PositionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PositionAttributeConfiguration : IEntityTypeConfiguration<PositionAttribute>
{
    public void Configure(EntityTypeBuilder<PositionAttribute> builder)
    {
        builder.HasIndex(pa => new { pa.PositionId, pa.AttributeDefinitionId }).IsUnique();
        builder.HasIndex(pa => pa.AttributeDefinitionId);

        builder.HasOne(pa => pa.AttributeDefinition)
               .WithMany(a => a.PositionAttributes)
               .HasForeignKey(pa => pa.AttributeDefinitionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PositionProjectTagConfiguration : IEntityTypeConfiguration<PositionProjectTag>
{
    public void Configure(EntityTypeBuilder<PositionProjectTag> builder)
    {
        builder.HasIndex(pt => new { pt.PositionId, pt.Tag }).IsUnique();
        builder.HasIndex(pt => pt.Tag);
        builder.Property(pt => pt.Tag).HasMaxLength(100).IsRequired();
    }
}

public class PositionAccessRuleConfiguration : IEntityTypeConfiguration<PositionAccessRule>
{
    public void Configure(EntityTypeBuilder<PositionAccessRule> builder)
    {
        builder.HasIndex(ar => ar.PositionId);
        builder.HasIndex(ar => ar.AttributeDefinitionId);

        builder.Property(ar => ar.Value).HasMaxLength(500).IsRequired();

        builder.HasOne(ar => ar.AttributeDefinition)
               .WithMany(a => a.AccessRules)
               .HasForeignKey(ar => ar.AttributeDefinitionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
