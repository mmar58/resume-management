namespace backend.Domain.Entities;

/// <summary>
/// A Position is a customizable CV template managed by Recruiters.
/// All Recruiters share the same pool — there is no ownership concept (Section 10).
/// </summary>
public class Position
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Company { get; set; }
    public string? Level { get; set; }  // e.g. Junior, Middle, Senior, C-level

    /// <summary>
    /// Maximum number of projects that will appear in generated CVs.
    /// 0 means unlimited.
    /// </summary>
    public int MaxProjects { get; set; } = 0;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Optimistic concurrency token (Section 16).</summary>
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[] RowVersion { get; set; } = [];

    // Navigation
    public ICollection<PositionAttribute> PositionAttributes { get; set; } = [];
    public ICollection<PositionProjectTag> ProjectTags { get; set; } = [];
    public ICollection<PositionAccessRule> AccessRules { get; set; } = [];
    public ICollection<CV> CVs { get; set; } = [];
    public ICollection<DiscussionPost> DiscussionPosts { get; set; } = [];
}
