using backend.Domain.Enums;

namespace backend.Domain.Entities;

/// <summary>
/// An attribute-based filter rule on a Position (Section 11).
/// Multiple rules on the same position are combined with AND.
/// The available operators depend on the attribute's DataType and are enforced server-side.
/// </summary>
public class PositionAccessRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PositionId { get; set; }
    public Guid AttributeDefinitionId { get; set; }
    public AccessRuleOperator Operator { get; set; }

    /// <summary>
    /// The comparison value stored as a string and cast at evaluation time
    /// based on the attribute's DataType.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    // Navigation
    public Position Position { get; set; } = null!;
    public AttributeDefinition AttributeDefinition { get; set; } = null!;
}
