using backend.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace backend.Domain.Entities;

/// <summary>
/// Application user. Extends IdentityUser to leverage ASP.NET Core Identity
/// for password hashing, lockout, and claims.
/// </summary>
public class User : IdentityUser<Guid>
{
    public string? DisplayName { get; set; }
    public UserRole Role { get; set; } = UserRole.Candidate;
    public bool IsBlocked { get; set; } = false;
    public string? PreferredLocale { get; set; } = "en";
    public string? PreferredTheme { get; set; } = "light";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastSeenAt { get; set; }

    // Navigation
    public CandidateProfile? CandidateProfile { get; set; }
    public ICollection<SocialAccount> SocialAccounts { get; set; } = [];
    public ICollection<CVLike> CVLikes { get; set; } = [];
    public ICollection<DiscussionPost> DiscussionPosts { get; set; } = [];
    public ICollection<RecentlyUsedAttribute> RecentlyUsedAttributes { get; set; } = [];
}
