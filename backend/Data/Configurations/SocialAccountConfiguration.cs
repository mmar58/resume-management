using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class SocialAccountConfiguration : IEntityTypeConfiguration<SocialAccount>
{
    public void Configure(EntityTypeBuilder<SocialAccount> builder)
    {
        // A user cannot link the same provider twice
        builder.HasIndex(sa => new { sa.UserId, sa.Provider }).IsUnique();
        // Fast lookup by provider + external key
        builder.HasIndex(sa => new { sa.Provider, sa.ProviderKey }).IsUnique();

        builder.Property(sa => sa.ProviderKey).HasMaxLength(500).IsRequired();
        builder.Property(sa => sa.Email).HasMaxLength(320);
    }
}
