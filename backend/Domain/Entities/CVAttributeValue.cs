namespace backend.Domain.Entities;

/// <summary>
/// Join entity representing a CandidateAttributeValue selected for inclusion in a specific CV.
/// By storing the relation to CandidateAttributeValue, if the user updates the attribute value
/// in their profile, it automatically reflects on the CV.
/// </summary>
public class CVAttributeValue
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CVId { get; set; }
    public Guid CandidateAttributeValueId { get; set; }

    // Navigation
    public CV CV { get; set; } = null!;
    public CandidateAttributeValue CandidateAttributeValue { get; set; } = null!;
}
