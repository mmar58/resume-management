using backend.Application.DTOs.Search;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Services;

public class SearchService : ISearchService
{
    private readonly AppDbContext _db;

    public SearchService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<GlobalSearchResponse> SearchAsync(GlobalSearchRequest request, bool isRecruiter, CancellationToken ct = default)
    {
        var positions = new List<PositionSearchResult>();
        var candidates = new List<CandidateSearchResult>();

        if (string.IsNullOrWhiteSpace(request.Query))
            return new GlobalSearchResponse(positions, candidates);

        var query = request.Query.Trim().ToLowerInvariant();

        if (request.IncludePositions)
        {
            var pQuery = _db.Positions.AsNoTracking().AsQueryable();
            
            // Non-recruiters can only see active positions
            if (!isRecruiter)
            {
                pQuery = pQuery.Where(p => p.IsActive);
            }

            positions = await pQuery
                .Where(p => p.Title.ToLower().Contains(query) || 
                            (p.Company != null && p.Company.ToLower().Contains(query)) ||
                            (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(query)))
                .Take(20)
                .Select(p => new PositionSearchResult(
                    p.Id,
                    p.Title,
                    p.Company,
                    p.ShortDescription
                ))
                .ToListAsync(ct);
        }

        if (request.IncludeCandidates && isRecruiter) // Only recruiters can search candidate profiles globally
        {
            candidates = await _db.CandidateProfiles
                .AsNoTracking()
                .Where(c => (c.FirstName + " " + c.LastName).ToLower().Contains(query) ||
                            (c.Location != null && c.Location.ToLower().Contains(query)))
                .Take(20)
                .Select(c => new CandidateSearchResult(
                    c.Id,
                    (c.FirstName + " " + c.LastName).Trim(),
                    c.Location,
                    c.PhotoUrl
                ))
                .ToListAsync(ct);
        }

        return new GlobalSearchResponse(positions, candidates);
    }
}
