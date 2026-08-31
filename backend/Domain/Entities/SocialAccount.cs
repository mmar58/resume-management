using backend.Domain.Enums;

namespace backend.Domain.Entities;

/// <summary>
/// Links a User to an external OAuth provider account.
/// A single User can have multiple social accounts.
/// </summary>
public class SocialAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public OAuthProvider Provider { get; set; }
    public string ProviderKey { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime LinkedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
}
