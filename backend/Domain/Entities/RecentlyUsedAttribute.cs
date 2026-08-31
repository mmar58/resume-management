namespace backend.Domain.Entities;

/// <summary>
/// Tracks recently used attributes per user to power the "recently used"
/// section of the AttributePicker (Section 9).
/// </summary>
public class RecentlyUsedAttribute
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid AttributeDefinitionId { get; set; }
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public AttributeDefinition AttributeDefinition { get; set; } = null!;
}
