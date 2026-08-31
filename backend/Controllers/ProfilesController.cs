using backend.Application.DTOs.Profile;
using backend.Application.Services;
using backend.Infrastructure.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

/// <summary>
/// Manages the authenticated candidate's profile and attribute values.
/// All endpoints require authentication; data is always scoped to the current user.
/// </summary>
[Authorize]
public class ProfilesController : ApiControllerBase
{
    private readonly IProfileService _profileService;
    private readonly IFileStorageService _storage;

    public ProfilesController(IProfileService profileService, IFileStorageService storage)
    {
        _profileService = profileService;
        _storage = storage;
    }

    // GET /api/profiles/me
    [HttpGet("me")]
    [ProducesResponseType(typeof(ProfileResponse), 200)]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
    {
        var profile = await _profileService.GetProfileAsync(CurrentUserId, ct);
        return Ok(profile);
    }

    // PUT /api/profiles/me
    [HttpPut("me")]
    [ProducesResponseType(typeof(ProfileResponse), 200)]
    [ProducesResponseType(409)]  // Concurrency conflict
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var profile = await _profileService.UpdateProfileAsync(CurrentUserId, request, ct);
        return Ok(profile);
    }

    // POST /api/profiles/me/photo
    [HttpPost("me/photo")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UploadPhoto(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest("Only JPEG, PNG, WebP, and GIF images are supported.");

        if (file.Length > 5 * 1024 * 1024)  // 5 MB limit
            return BadRequest("Image must be smaller than 5 MB.");

        await using var stream = file.OpenReadStream();
        var url = await _storage.UploadAsync(stream, file.FileName, "profile-photos", ct);

        return Ok(new { url });
    }

    // ── Attribute Values ──────────────────────────────────────────────────────

    // GET /api/profiles/me/attributes
    [HttpGet("me/attributes")]
    [ProducesResponseType(typeof(List<AttributeValueResponse>), 200)]
    public async Task<IActionResult> GetAttributes(CancellationToken ct)
    {
        var values = await _profileService.GetAttributeValuesAsync(CurrentUserId, ct);
        return Ok(values);
    }

    // POST /api/profiles/me/attributes
    [HttpPost("me/attributes")]
    [ProducesResponseType(typeof(AttributeValueResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AddAttribute([FromBody] AddAttributeToProfileRequest request, CancellationToken ct)
    {
        var value = await _profileService.AddAttributeToProfileAsync(CurrentUserId, request, ct);
        return StatusCode(201, value);
    }

    // PUT /api/profiles/me/attributes/{id}
    [HttpPut("me/attributes/{id:guid}")]
    [ProducesResponseType(typeof(AttributeValueResponse), 200)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> UpdateAttribute(Guid id, [FromBody] UpdateAttributeValueRequest request, CancellationToken ct)
    {
        var value = await _profileService.UpdateAttributeValueAsync(CurrentUserId, id, request, ct);
        return Ok(value);
    }

    // DELETE /api/profiles/me/attributes/{id}
    [HttpDelete("me/attributes/{id:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> RemoveAttribute(Guid id, CancellationToken ct)
    {
        await _profileService.RemoveAttributeFromProfileAsync(CurrentUserId, id, ct);
        return NoContent();
    }
}
