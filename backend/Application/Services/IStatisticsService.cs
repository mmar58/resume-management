using backend.Application.DTOs.Dashboard;

namespace backend.Application.Services;

public interface IStatisticsService
{
    Task<DashboardStatisticsResponse> GetDashboardStatisticsAsync(CancellationToken ct = default);
}
