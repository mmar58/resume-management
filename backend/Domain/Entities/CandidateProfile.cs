namespace backend.Domain.Entities;

/// <summary>
/// Candidate profile — one per user.
/// Holds built-in required attributes (FirstName, LastName, Location, PhotoUrl)
/// and acts as the root aggregate for CandidateAttributeValues, Projects, and CVs.
/// </summary>
public class CandidateProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }

    // --- Built-in mandatory attributes ---
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Location { get; set; }
    public string? PhotoUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// EF Core concurrency token for optimistic locking (Section 16).
    /// </summary>
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[] RowVersion { get; set; } = [];

    // Navigation
    public User User { get; set; } = null!;
    public ICollection<CandidateAttributeValue> AttributeValues { get; set; } = [];
    public ICollection<Project> Projects { get; set; } = [];
    public ICollection<CV> CVs { get; set; } = [];
}
