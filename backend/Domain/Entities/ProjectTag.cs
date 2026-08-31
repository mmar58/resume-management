namespace backend.Domain.Entities;

/// <summary>
/// Technology tag on a Project. Stored as a separate table to support
/// autocomplete queries (DISTINCT tags LIKE 'prefix%').
/// </summary>
public class ProjectTag
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public string Tag { get; set; } = string.Empty;

    // Navigation
    public Project Project { get; set; } = null!;
}
