namespace backend.Application.DTOs.Positions;

using backend.Domain.Enums;
using backend.Application.DTOs.Attributes;

// ── Position Responses ───────────────────────────────────────────────────────

public record PositionResponse(
    Guid Id,
    string Title,
    string? ShortDescription,
    string? Company,
    string? Level,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string RowVersion,
    List<PositionAttributeResponse> Attributes,
    List<string> ProjectTags,
    List<PositionAccessRuleResponse> AccessRules
);

public record PositionSummaryResponse(
    Guid Id,
    string Title,
    string? Company,
    string? Level,
    bool IsActive,
    DateTime CreatedAt,
    int RequiredAttributesCount,
    int AccessRulesCount
);

public record PositionAttributeResponse(
    Guid Id,
    Guid AttributeDefinitionId,
    string AttributeName,
    AttributeDataType DataType
);

public record PositionAccessRuleResponse(
    Guid Id,
    Guid AttributeDefinitionId,
    string AttributeName,
    AccessRuleOperator Operator,
    string Value
);

// ── Create/Update Requests ───────────────────────────────────────────────────

public record CreatePositionRequest(
    string Title,
    string? ShortDescription,
    string? Company,
    string? Level,
    List<Guid>? AttributeDefinitionIds,
    List<string>? ProjectTags,
    List<CreateAccessRuleRequest>? AccessRules
);

public record UpdatePositionRequest(
    string Title,
    string? ShortDescription,
    string? Company,
    string? Level,
    bool IsActive,
    List<Guid>? AttributeDefinitionIds,
    List<string>? ProjectTags,
    List<CreateAccessRuleRequest>? AccessRules,
    string RowVersion
);

public record CreateAccessRuleRequest(
    Guid AttributeDefinitionId,
    AccessRuleOperator Operator,
    string Value
);
