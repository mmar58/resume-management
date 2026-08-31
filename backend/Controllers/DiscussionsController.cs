using backend.Application.DTOs.Common;
using backend.Application.DTOs.Discussions;
using backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

/// <summary>
/// Manages discussions on Positions (for Recruiters and Administrators).
/// </summary>
[Authorize(Policy = "RequireRecruiter")]
[Route("api/positions/{positionId:guid}/discussions")]
public class DiscussionsController : ApiControllerBase
{
    private readonly IDiscussionService _discussionService;

    public DiscussionsController(IDiscussionService discussionService)
    {
        _discussionService = discussionService;
    }

    // GET /api/positions/{positionId}/discussions
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<DiscussionPostResponse>), 200)]
    public async Task<IActionResult> GetDiscussions(
        Guid positionId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var discussions = await _discussionService.GetPositionDiscussionsAsync(positionId, page, pageSize, ct);
        return Ok(discussions);
    }

    // POST /api/positions/{positionId}/discussions
    [HttpPost]
    [ProducesResponseType(typeof(DiscussionPostResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CreateDiscussion(Guid positionId, [FromBody] CreateDiscussionPostRequest request, CancellationToken ct)
    {
        var post = await _discussionService.CreatePostAsync(positionId, CurrentUserId, request, ct);
        // Note: Creating a post automatically broadcasts it via SignalR to connected clients
        return StatusCode(201, post);
    }
}
