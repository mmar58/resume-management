using backend.Application.DTOs.Common;
using backend.Application.DTOs.Positions;

namespace backend.Application.Services;

public interface IPositionService
{
    Task<PagedResponse<PositionSummaryResponse>> GetPositionsAsync(bool onlyActive = true, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<PositionResponse> GetPositionByIdAsync(Guid id, CancellationToken ct = default);
    Task<PositionResponse> CreatePositionAsync(CreatePositionRequest request, CancellationToken ct = default);
    Task<PositionResponse> DuplicatePositionAsync(Guid id, CancellationToken ct = default);
    Task<PositionResponse> UpdatePositionAsync(Guid id, UpdatePositionRequest request, CancellationToken ct = default);
    Task DeletePositionAsync(Guid id, CancellationToken ct = default);
}
