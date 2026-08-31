namespace backend.Domain.Entities;

/// <summary>
/// Predefined option for OneOfMany attributes.
/// </summary>
public class AttributeOption
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AttributeDefinitionId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int Order { get; set; } = 0;

    // Navigation
    public AttributeDefinition AttributeDefinition { get; set; } = null!;
}
