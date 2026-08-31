using backend.Application.DTOs.Search;
using backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

/// <summary>
/// Global search across positions and candidate profiles.
/// </summary>
[Authorize]
public class SearchController : ApiControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    // GET /api/search
    [HttpGet]
    [ProducesResponseType(typeof(GlobalSearchResponse), 200)]
    public async Task<IActionResult> GlobalSearch(
        [FromQuery] string q,
        [FromQuery] bool positions = true,
        [FromQuery] bool candidates = true,
        CancellationToken ct = default)
    {
        var isRecruiter = User.IsInRole("Recruiter") || User.IsInRole("Administrator");
        
        var request = new GlobalSearchRequest(q, positions, candidates);
        var response = await _searchService.SearchAsync(request, isRecruiter, ct);
        
        return Ok(response);
    }
}
