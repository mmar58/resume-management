namespace backend.Domain.Entities;

/// <summary>
/// A candidate's portfolio project — reusable across all CVs.
/// </summary>
public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CandidateProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    /// <summary>Markdown-formatted description.</summary>
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Optimistic concurrency token (Section 16).</summary>
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[] RowVersion { get; set; } = [];

    // Navigation
    public CandidateProfile CandidateProfile { get; set; } = null!;
    public ICollection<ProjectTag> Tags { get; set; } = [];
}
