namespace backend.Domain.Entities;

/// <summary>
/// Discussion post on a Position.
/// Posts are chronological; new posts append only.
/// </summary>
public class DiscussionPost
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PositionId { get; set; }
    public Guid AuthorId { get; set; }

    /// <summary>Markdown-formatted content.</summary>
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Position Position { get; set; } = null!;
    public User Author { get; set; } = null!;
}
