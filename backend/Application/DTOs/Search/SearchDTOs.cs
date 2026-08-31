namespace backend.Application.DTOs.Search;

public record GlobalSearchRequest(
    string Query,
    bool IncludePositions = true,
    bool IncludeCandidates = true
);

public record GlobalSearchResponse(
    List<PositionSearchResult> Positions,
    List<CandidateSearchResult> Candidates
);

public record PositionSearchResult(
    Guid Id,
    string Title,
    string? Company,
    string? ShortDescription
);

public record CandidateSearchResult(
    Guid Id,
    string Name,
    string? Location,
    string? PhotoUrl
);
