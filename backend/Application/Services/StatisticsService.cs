using backend.Application.DTOs.Dashboard;
using backend.Data;
using backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Services;

public class StatisticsService : IStatisticsService
{
    private readonly AppDbContext _db;

    public StatisticsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardStatisticsResponse> GetDashboardStatisticsAsync(CancellationToken ct = default)
    {
        var totalCandidates = await _db.CandidateProfiles.CountAsync(ct);
        var totalActivePositions = await _db.Positions.CountAsync(p => p.IsActive, ct);
        var totalCVsSubmitted = await _db.CVs.CountAsync(c => c.Status == CVStatus.Published, ct);
        var totalDiscussions = await _db.DiscussionPosts.CountAsync(ct);

        // Popular positions (based on number of submitted CVs)
        var popularPositions = await _db.Positions
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderByDescending(p => _db.CVs.Count(c => c.PositionId == p.Id && c.Status == CVStatus.Published))
            .Take(5)
            .Select(p => new PopularPositionResponse(
                p.Id,
                p.Title,
                p.Company,
                _db.CVs.Count(c => c.PositionId == p.Id && c.Status == CVStatus.Published)
            ))
            .ToListAsync(ct);

        // Tag cloud
        var topTags = await _db.PositionProjectTags
            .AsNoTracking()
            .GroupBy(t => t.Tag)
            .Select(g => new TagCloudItem(g.Key, g.Count()))
            .OrderByDescending(t => t.Count)
            .Take(20)
            .ToListAsync(ct);

        return new DashboardStatisticsResponse(
            totalCandidates,
            totalActivePositions,
            totalCVsSubmitted,
            totalDiscussions,
            popularPositions,
            topTags
        );
    }
}
