using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.Role);

        builder.Property(u => u.DisplayName).HasMaxLength(200);
        builder.Property(u => u.PreferredLocale).HasMaxLength(10).HasDefaultValue("en");
        builder.Property(u => u.PreferredTheme).HasMaxLength(20).HasDefaultValue("light");

        builder.HasOne(u => u.CandidateProfile)
               .WithOne(p => p.User)
               .HasForeignKey<CandidateProfile>(p => p.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.SocialAccounts)
               .WithOne(sa => sa.User)
               .HasForeignKey(sa => sa.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.CVLikes)
               .WithOne(l => l.Recruiter)
               .HasForeignKey(l => l.RecruiterId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.DiscussionPosts)
               .WithOne(dp => dp.Author)
               .HasForeignKey(dp => dp.AuthorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.RecentlyUsedAttributes)
               .WithOne(r => r.User)
               .HasForeignKey(r => r.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
