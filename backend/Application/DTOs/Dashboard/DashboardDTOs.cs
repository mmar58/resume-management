namespace backend.Application.DTOs.Dashboard;

public record DashboardStatisticsResponse(
    int TotalCandidates,
    int TotalActivePositions,
    int TotalCVsSubmitted,
    int TotalDiscussions,
    List<PopularPositionResponse> PopularPositions,
    List<TagCloudItem> TopTags
);

public record PopularPositionResponse(
    Guid Id,
    string Title,
    string? Company,
    int ApplicantCount
);

public record TagCloudItem(
    string Tag,
    int Count
);
