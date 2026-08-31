using backend.Domain.Enums;

namespace backend.Application.DTOs.Profile;

// ── Profile ──────────────────────────────────────────────────────────────────

public record ProfileResponse(
    Guid Id,
    Guid UserId,
    string? FirstName,
    string? LastName,
    string? Location,
    string? PhotoUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string RowVersion  // Base64-encoded for optimistic locking
);

public record UpdateProfileRequest(
    string? FirstName,
    string? LastName,
    string? Location,
    string? PhotoUrl,
    string RowVersion  // Current version — required for optimistic locking
);

// ── Attribute Values ──────────────────────────────────────────────────────────

public record AttributeValueResponse(
    Guid Id,
    Guid AttributeDefinitionId,
    string AttributeName,
    string? AttributeCategory,
    AttributeDataType DataType,
    string? StringValue,
    string? TextValue,
    string? ImageUrl,
    decimal? NumericValue,
    DateTime? DateValue,
    DateTime? DateEndValue,
    bool? BoolValue,
    string? OptionValue,
    string RowVersion
);

public record AddAttributeToProfileRequest(
    Guid AttributeDefinitionId
);

public record UpdateAttributeValueRequest(
    string? StringValue,
    string? TextValue,
    string? ImageUrl,
    decimal? NumericValue,
    DateTime? DateValue,
    DateTime? DateEndValue,
    bool? BoolValue,
    string? OptionValue,
    string RowVersion  // For optimistic locking
);
