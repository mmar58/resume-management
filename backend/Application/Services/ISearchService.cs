using backend.Application.DTOs.Search;

namespace backend.Application.Services;

public interface ISearchService
{
    Task<GlobalSearchResponse> SearchAsync(GlobalSearchRequest request, bool isRecruiter, CancellationToken ct = default);
}
