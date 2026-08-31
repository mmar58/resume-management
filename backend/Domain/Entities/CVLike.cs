namespace backend.Domain.Entities;

/// <summary>
/// A Recruiter's like on a published CV.
/// Unique constraint on (CVId, RecruiterId) enforced at the database level (Section 19).
/// </summary>
public class CVLike
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CVId { get; set; }
    public Guid RecruiterId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public CV CV { get; set; } = null!;
    public User Recruiter { get; set; } = null!;
}
