using backend.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

/// <summary>
/// Main EF Core DbContext. Extends IdentityDbContext to include
/// ASP.NET Core Identity tables alongside custom domain entities.
/// All entity configurations are in Data/Configurations/.
/// </summary>
public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // --- Domain DbSets ---
    public DbSet<SocialAccount> SocialAccounts => Set<SocialAccount>();
    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();
    public DbSet<AttributeDefinition> AttributeDefinitions => Set<AttributeDefinition>();
    public DbSet<AttributeOption> AttributeOptions => Set<AttributeOption>();
    public DbSet<CandidateAttributeValue> CandidateAttributeValues => Set<CandidateAttributeValue>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectTag> ProjectTags => Set<ProjectTag>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<PositionAttribute> PositionAttributes => Set<PositionAttribute>();
    public DbSet<PositionProjectTag> PositionProjectTags => Set<PositionProjectTag>();
    public DbSet<PositionAccessRule> PositionAccessRules => Set<PositionAccessRule>();
    public DbSet<CV> CVs => Set<CV>();
    public DbSet<CVLike> CVLikes => Set<CVLike>();
    public DbSet<DiscussionPost> DiscussionPosts => Set<DiscussionPost>();
    public DbSet<RecentlyUsedAttribute> RecentlyUsedAttributes => Set<RecentlyUsedAttribute>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all IEntityTypeConfiguration<T> implementations from this assembly
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
