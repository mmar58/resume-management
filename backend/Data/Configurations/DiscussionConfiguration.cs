using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class DiscussionPostConfiguration : IEntityTypeConfiguration<DiscussionPost>
{
    public void Configure(EntityTypeBuilder<DiscussionPost> builder)
    {
        builder.HasIndex(dp => dp.PositionId);
        builder.HasIndex(dp => dp.CreatedAt);  // Chronological ordering

        builder.Property(dp => dp.Content).IsRequired();
    }
}

public class RecentlyUsedAttributeConfiguration : IEntityTypeConfiguration<RecentlyUsedAttribute>
{
    public void Configure(EntityTypeBuilder<RecentlyUsedAttribute> builder)
    {
        // One entry per (User, Attribute) — update UsedAt on re-use
        builder.HasIndex(r => new { r.UserId, r.AttributeDefinitionId }).IsUnique();
        builder.HasIndex(r => new { r.UserId, r.UsedAt });  // For "ORDER BY UsedAt DESC LIMIT 10"

        builder.HasOne(r => r.AttributeDefinition)
               .WithMany(a => a.RecentlyUsed)
               .HasForeignKey(r => r.AttributeDefinitionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
