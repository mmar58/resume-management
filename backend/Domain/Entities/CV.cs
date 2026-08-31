using backend.Domain.Enums;

namespace backend.Domain.Entities;

/// <summary>
/// A candidate CV for a specific position.
///
/// A CV is a VIEW SPECIFICATION — it holds only which attributes and projects
/// to display, plus publication state. Attribute values always come from
/// CandidateAttributeValue (single source of truth, Section 13).
///
/// Unique constraint: one CV per (CandidateProfileId, PositionId).
/// </summary>
public class CV
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CandidateProfileId { get; set; }
    public Guid PositionId { get; set; }
    public CVStatus Status { get; set; } = CVStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Optimistic concurrency token (Section 16).</summary>
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[] RowVersion { get; set; } = [];

    // Navigation
    public CandidateProfile CandidateProfile { get; set; } = null!;
    public Position Position { get; set; } = null!;
    public ICollection<CVLike> Likes { get; set; } = [];
    public ICollection<CVAttributeValue> SelectedAttributes { get; set; } = [];
    public ICollection<CVProject> SelectedProjects { get; set; } = [];
}
