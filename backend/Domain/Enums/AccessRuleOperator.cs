namespace backend.Domain.Enums;

/// <summary>
/// Operators used in Position access rules.
/// Available operators depend on the attribute's data type.
/// </summary>
public enum AccessRuleOperator
{
    // Universal
    Equals = 1,
    NotEquals = 2,

    // Numeric / Date
    GreaterThan = 3,
    GreaterThanOrEqual = 4,
    LessThan = 5,
    LessThanOrEqual = 6,

    // String / Text
    Contains = 7,
    StartsWith = 8,
    EndsWith = 9,

    // Boolean (use Equals = true/false)
    IsTrue = 10,
    IsFalse = 11
}
