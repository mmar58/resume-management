namespace backend.Application.DTOs.Common;

/// <summary>
/// Standard paginated response wrapper.
/// </summary>
public record PagedResponse<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrev => Page > 1;
}

/// <summary>
/// Standard error response shape.
/// </summary>
public record ErrorResponse(string Error, int StatusCode);

/// <summary>
/// Standard message response for operations that return no data.
/// </summary>
public record MessageResponse(string Message);
