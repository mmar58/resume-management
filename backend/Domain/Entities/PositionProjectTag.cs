namespace backend.Domain.Entities;

/// <summary>
/// Technology tag filter on a Position — used to select relevant candidate projects
/// for CV generation. Projects with matching tags are included (up to MaxProjects).
/// </summary>
public class PositionProjectTag
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PositionId { get; set; }
    public string Tag { get; set; } = string.Empty;

    // Navigation
    public Position Position { get; set; } = null!;
}
