namespace backend.Domain.Enums;

/// <summary>
/// Supported data types for Attribute Library attributes.
/// These control UI rendering and typed value storage.
/// </summary>
public enum AttributeDataType
{
    /// <summary>Single-line plain text.</summary>
    String = 1,

    /// <summary>Markdown-formatted multi-line text.</summary>
    Text = 2,

    /// <summary>External cloud-stored image URL.</summary>
    Image = 3,

    /// <summary>Numeric (decimal) value.</summary>
    Numeric = 4,

    /// <summary>Single date value.</summary>
    Date = 5,

    /// <summary>Date range: start + end date.</summary>
    Period = 6,

    /// <summary>Boolean (checkbox).</summary>
    Boolean = 7,

    /// <summary>Single selection from a predefined list of options.</summary>
    OneOfMany = 8
}
