namespace backend.Domain.Entities;

/// <summary>
/// An attribute from the shared library that is required/included in a Position's CV template.
/// </summary>
public class PositionAttribute
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PositionId { get; set; }
    public Guid AttributeDefinitionId { get; set; }

    /// <summary>Whether this attribute is mandatory for CV publication.</summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>Display order within the CV.</summary>
    public int Order { get; set; } = 0;

    // Navigation
    public Position Position { get; set; } = null!;
    public AttributeDefinition AttributeDefinition { get; set; } = null!;
}
