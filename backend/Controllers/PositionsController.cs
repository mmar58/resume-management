using backend.Application.DTOs.Common;
using backend.Application.DTOs.Positions;
using backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

/// <summary>
/// Manages positions (job posts).
/// Read operations are available to all authenticated users (Candidates viewing open positions).
/// Write operations (Create/Update/Delete) are restricted to Recruiters and Administrators.
/// </summary>
[Authorize]
public class PositionsController : ApiControllerBase
{
    private readonly IPositionService _positionService;

    public PositionsController(IPositionService positionService)
    {
        _positionService = positionService;
    }

    // GET /api/positions
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<PositionSummaryResponse>), 200)]
    public async Task<IActionResult> GetPositions(
        [FromQuery] bool onlyActive = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var positions = await _positionService.GetPositionsAsync(onlyActive, page, pageSize, ct);
        return Ok(positions);
    }

    // GET /api/positions/{id}
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PositionResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetPosition(Guid id, CancellationToken ct)
    {
        var position = await _positionService.GetPositionByIdAsync(id, ct);
        return Ok(position);
    }

    // POST /api/positions
    [HttpPost]
    [Authorize(Policy = "RequireRecruiter")]
    [ProducesResponseType(typeof(PositionResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreatePosition([FromBody] CreatePositionRequest request, CancellationToken ct)
    {
        var position = await _positionService.CreatePositionAsync(request, ct);
        return StatusCode(201, position);
    }

    // POST /api/positions/{id}/duplicate
    [HttpPost("{id:guid}/duplicate")]
    [Authorize(Policy = "RequireRecruiter")]
    [ProducesResponseType(typeof(PositionResponse), 201)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DuplicatePosition(Guid id, CancellationToken ct)
    {
        var position = await _positionService.DuplicatePositionAsync(id, ct);
        return StatusCode(201, position);
    }

    // PUT /api/positions/{id}
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireRecruiter")]
    [ProducesResponseType(typeof(PositionResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> UpdatePosition(Guid id, [FromBody] UpdatePositionRequest request, CancellationToken ct)
    {
        var position = await _positionService.UpdatePositionAsync(id, request, ct);
        return Ok(position);
    }

    // DELETE /api/positions/{id}
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireRecruiter")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeletePosition(Guid id, CancellationToken ct)
    {
        await _positionService.DeletePositionAsync(id, ct);
        return NoContent();
    }
}
