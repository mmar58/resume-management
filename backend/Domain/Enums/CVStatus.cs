namespace backend.Domain.Enums;

/// <summary>
/// Publication state of a Candidate CV.
/// Designed to be extensible — additional states can be added without breaking existing logic.
/// </summary>
public enum CVStatus
{
    Draft = 0,
    Published = 1
}
