using backend.Domain.Enums;

namespace backend.Domain.Entities;

/// <summary>
/// Shared Attribute Library definition.
/// Recruiters manage these; they are reused across profiles, positions, and CVs.
/// Names are globally unique.
/// </summary>
public class AttributeDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Globally unique display name (indexed).</summary>
    public string Name { get; set; } = string.Empty;

    public string? Category { get; set; }
    public string? Description { get; set; }
    public AttributeDataType DataType { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<AttributeOption> Options { get; set; } = [];
    public ICollection<CandidateAttributeValue> CandidateValues { get; set; } = [];
    public ICollection<PositionAttribute> PositionAttributes { get; set; } = [];
    public ICollection<PositionAccessRule> AccessRules { get; set; } = [];
    public ICollection<RecentlyUsedAttribute> RecentlyUsed { get; set; } = [];
}
