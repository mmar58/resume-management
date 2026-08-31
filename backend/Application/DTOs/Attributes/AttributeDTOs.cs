namespace backend.Application.DTOs.Attributes;

using backend.Domain.Enums;

public record AttributeResponse(
    Guid Id,
    string Name,
    string? Category,
    string? Description,
    AttributeDataType DataType,
    List<string> Options,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateAttributeRequest(
    string Name,
    string? Category,
    string? Description,
    AttributeDataType DataType,
    List<string>? Options
);

public record UpdateAttributeRequest(
    string Name,
    string? Category,
    string? Description,
    List<string>? Options
);
