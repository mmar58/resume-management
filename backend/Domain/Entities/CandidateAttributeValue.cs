namespace backend.Domain.Entities;

/// <summary>
/// Master value store for a candidate's attribute.
/// This is the SINGLE SOURCE OF TRUTH for attribute values (Section 13).
/// Editing from a CV updates this record; CVs never store their own value copies.
///
/// Uses typed nullable columns instead of a single VARCHAR to enable
/// strongly-typed access rule comparisons (Section 11).
/// </summary>
public class CandidateAttributeValue
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CandidateProfileId { get; set; }
    public Guid AttributeDefinitionId { get; set; }

    // Typed value columns — only the column matching the attribute's DataType is populated.
    public string? StringValue { get; set; }
    public string? TextValue { get; set; }       // Markdown text
    public string? ImageUrl { get; set; }         // Cloud storage URL
    public decimal? NumericValue { get; set; }
    public DateTime? DateValue { get; set; }
    public DateTime? DateEndValue { get; set; }  // For Period type
    public bool? BoolValue { get; set; }
    public string? OptionValue { get; set; }     // Stored value for OneOfMany

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Optimistic concurrency token (Section 16).</summary>
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[] RowVersion { get; set; } = [];

    // Navigation
    public CandidateProfile CandidateProfile { get; set; } = null!;
    public AttributeDefinition AttributeDefinition { get; set; } = null!;
}
