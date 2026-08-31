using backend.Application.DTOs.Common;
using backend.Application.DTOs.CVs;
using backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

/// <summary>
/// Manages CV creation/submission (for candidates) and review/liking (for recruiters).
/// </summary>
[Authorize]
public class CVsController : ApiControllerBase
{
    private readonly ICVService _cvService;

    public CVsController(ICVService cvService)
    {
        _cvService = cvService;
    }

    // ── Candidate Endpoints ───────────────────────────────────────────────────

    // GET /api/cvs/me
    [HttpGet("me")]
    [Authorize(Policy = "RequireCandidate")]
    [ProducesResponseType(typeof(PagedResponse<CVSummaryResponse>), 200)]
    public async Task<IActionResult> GetMyCVs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var cvs = await _cvService.GetCandidateCVsAsync(CurrentUserId, page, pageSize, ct);
        return Ok(cvs);
    }

    // GET /api/cvs/me/{id}
    [HttpGet("me/{id:guid}")]
    [Authorize(Policy = "RequireCandidate")]
    [ProducesResponseType(typeof(CVResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetMyCV(Guid id, CancellationToken ct)
    {
        var cv = await _cvService.GetCandidateCVAsync(CurrentUserId, id, ct);
        return Ok(cv);
    }

    // POST /api/cvs/me
    [HttpPost("me")]
    [Authorize(Policy = "RequireCandidate")]
    [ProducesResponseType(typeof(CVResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateMyCV([FromBody] CreateCVRequest request, CancellationToken ct)
    {
        var cv = await _cvService.CreateCVAsync(CurrentUserId, request, ct);
        return StatusCode(201, cv);
    }

    // PUT /api/cvs/me/{id}
    [HttpPut("me/{id:guid}")]
    [Authorize(Policy = "RequireCandidate")]
    [ProducesResponseType(typeof(CVResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> UpdateMyCV(Guid id, [FromBody] UpdateCVRequest request, CancellationToken ct)
    {
        var cv = await _cvService.UpdateCVAsync(CurrentUserId, id, request, ct);
        return Ok(cv);
    }

    // PATCH /api/cvs/me/{id}/status
    [HttpPatch("me/{id:guid}/status")]
    [Authorize(Policy = "RequireCandidate")]
    [ProducesResponseType(typeof(CVResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> ChangeMyCVStatus(Guid id, [FromBody] ChangeCVStatusRequest request, CancellationToken ct)
    {
        var cv = await _cvService.ChangeCVStatusAsync(CurrentUserId, id, request, ct);
        return Ok(cv);
    }

    // ── Recruiter Endpoints ───────────────────────────────────────────────────

    // GET /api/cvs/submitted
    [HttpGet("submitted")]
    [Authorize(Policy = "RequireRecruiter")]
    [ProducesResponseType(typeof(PagedResponse<CVSummaryResponse>), 200)]
    public async Task<IActionResult> GetSubmittedCVs(
        [FromQuery] Guid? positionId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var cvs = await _cvService.GetSubmittedCVsAsync(CurrentUserId, positionId, page, pageSize, ct);
        return Ok(cvs);
    }

    // GET /api/cvs/submitted/{id}
    [HttpGet("submitted/{id:guid}")]
    [Authorize(Policy = "RequireRecruiter")]
    [ProducesResponseType(typeof(CVResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSubmittedCV(Guid id, CancellationToken ct)
    {
        var cv = await _cvService.GetCVForReviewAsync(CurrentUserId, id, ct);
        return Ok(cv);
    }

    // POST /api/cvs/submitted/{id}/like
    [HttpPost("submitted/{id:guid}/like")]
    [Authorize(Policy = "RequireRecruiter")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ToggleLike(Guid id, CancellationToken ct)
    {
        await _cvService.ToggleCVLikeAsync(CurrentUserId, id, ct);
        return NoContent();
    }
}
