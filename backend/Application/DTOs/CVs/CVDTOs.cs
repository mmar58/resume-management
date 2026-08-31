namespace backend.Application.DTOs.CVs;

using backend.Domain.Enums;
using backend.Application.DTOs.Profile;
using backend.Application.DTOs.Projects;

// ── CV Responses ─────────────────────────────────────────────────────────────

public record CVSummaryResponse(
    Guid Id,
    Guid CandidateProfileId,
    Guid PositionId,
    string PositionTitle,
    string CandidateName,
    CVStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int LikeCount,
    bool HasLiked // Requires context of the requesting user
);

public record CVResponse(
    Guid Id,
    Guid CandidateProfileId,
    Guid PositionId,
    string PositionTitle,
    string CandidateName,
    string? CandidatePhotoUrl,
    string? CandidateLocation,
    CVStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int LikeCount,
    bool HasLiked,
    string RowVersion,
    List<AttributeValueResponse> SelectedAttributes,
    List<ProjectResponse> SelectedProjects
);

// ── Create/Update Requests ───────────────────────────────────────────────────

public record CreateCVRequest(
    Guid PositionId,
    List<Guid> SelectedAttributeValueIds,
    List<Guid> SelectedProjectIds
);

public record UpdateCVRequest(
    List<Guid> SelectedAttributeValueIds,
    List<Guid> SelectedProjectIds,
    string RowVersion
);

public record ChangeCVStatusRequest(
    CVStatus Status,
    string RowVersion
);
