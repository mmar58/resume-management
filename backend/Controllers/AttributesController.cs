using backend.Application.DTOs.Attributes;
using backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

/// <summary>
/// Manages the global attribute library.
/// Read access is available to all authenticated users (Candidates viewing available attributes, Recruiters viewing them).
/// Write access (Create/Update/Delete) is restricted to Recruiters and Administrators.
/// </summary>
[Authorize]
public class AttributesController : ApiControllerBase
{
    private readonly IAttributeService _attributeService;

    public AttributesController(IAttributeService attributeService)
    {
        _attributeService = attributeService;
    }

    // GET /api/attributes
    [HttpGet]
    [ProducesResponseType(typeof(List<AttributeResponse>), 200)]
    public async Task<IActionResult> GetAttributes(
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        CancellationToken ct = default)
    {
        var attributes = await _attributeService.GetAttributesAsync(search, category, ct);
        return Ok(attributes);
    }

    // GET /api/attributes/{id}
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AttributeResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAttribute(Guid id, CancellationToken ct)
    {
        var attribute = await _attributeService.GetAttributeByIdAsync(id, ct);
        return Ok(attribute);
    }

    // POST /api/attributes
    [HttpPost]
    [Authorize(Policy = "RequireRecruiter")]
    [ProducesResponseType(typeof(AttributeResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateAttribute([FromBody] CreateAttributeRequest request, CancellationToken ct)
    {
        var attribute = await _attributeService.CreateAttributeAsync(request, ct);
        return StatusCode(201, attribute);
    }

    // PUT /api/attributes/{id}
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireRecruiter")]
    [ProducesResponseType(typeof(AttributeResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateAttribute(Guid id, [FromBody] UpdateAttributeRequest request, CancellationToken ct)
    {
        var attribute = await _attributeService.UpdateAttributeAsync(id, request, ct);
        return Ok(attribute);
    }

    // DELETE /api/attributes/{id}
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireRecruiter")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteAttribute(Guid id, CancellationToken ct)
    {
        await _attributeService.DeleteAttributeAsync(id, ct);
        return NoContent();
    }

    // GET /api/attributes/recently-used
    [HttpGet("recently-used")]
    [ProducesResponseType(typeof(List<AttributeResponse>), 200)]
    public async Task<IActionResult> GetRecentlyUsedAttributes(CancellationToken ct)
    {
        var attributes = await _attributeService.GetRecentlyUsedAttributesAsync(CurrentUserId, ct);
        return Ok(attributes);
    }
    
    // GET /api/attributes/categories
    [HttpGet("categories")]
    [ProducesResponseType(typeof(List<string>), 200)]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var categories = await _attributeService.GetCategoriesAsync(ct);
        return Ok(categories);
    }
}
