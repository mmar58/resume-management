namespace backend.Application.DTOs.Projects;

public record ProjectResponse(
    Guid Id,
    string Name,
    DateTime? StartDate,
    DateTime? EndDate,
    string? Description,
    List<string> Tags,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string RowVersion
);

public record CreateProjectRequest(
    string Name,
    DateTime? StartDate,
    DateTime? EndDate,
    string? Description,
    List<string>? Tags
);

public record UpdateProjectRequest(
    string Name,
    DateTime? StartDate,
    DateTime? EndDate,
    string? Description,
    List<string>? Tags,
    string RowVersion  // For optimistic locking
);
