using backend.Application.DTOs.Dashboard;
using backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

/// <summary>
/// Dashboard statistics for Recruiters and Administrators.
/// </summary>
[Authorize(Policy = "RequireRecruiter")]
public class StatisticsController : ApiControllerBase
{
    private readonly IStatisticsService _statisticsService;

    public StatisticsController(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    // GET /api/statistics
    [HttpGet]
    [ProducesResponseType(typeof(DashboardStatisticsResponse), 200)]
    public async Task<IActionResult> GetDashboardStatistics(CancellationToken ct = default)
    {
        var stats = await _statisticsService.GetDashboardStatisticsAsync(ct);
        return Ok(stats);
    }
}
