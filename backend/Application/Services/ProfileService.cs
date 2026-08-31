using backend.Application.DTOs.Profile;
using backend.Data;
using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Services;

public class ProfileService : IProfileService
{
    private readonly AppDbContext _db;

    public ProfileService(AppDbContext db) => _db = db;

    // ── Profile ───────────────────────────────────────────────────────────────

    public async Task<ProfileResponse> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await _db.CandidateProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct)
            ?? throw new KeyNotFoundException("Profile not found.");

        return MapProfile(profile);
    }

    public async Task<ProfileResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var profile = await _db.CandidateProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId, ct)
            ?? throw new KeyNotFoundException("Profile not found.");

        // Verify client version matches DB version (optimistic locking)
        var clientVersion = Convert.FromBase64String(request.RowVersion);
        if (!clientVersion.SequenceEqual(profile.RowVersion))
            throw new Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException(
                "Profile was modified by another process. Please reload.", []);

        profile.FirstName = request.FirstName;
        profile.LastName = request.LastName;
        profile.Location = request.Location;
        profile.PhotoUrl = request.PhotoUrl;
        profile.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);  // EF will update RowVersion automatically

        return MapProfile(profile);
    }

    // ── Attribute Values ──────────────────────────────────────────────────────

    public async Task<List<AttributeValueResponse>> GetAttributeValuesAsync(Guid userId, CancellationToken ct = default)
    {
        var profileId = await GetProfileIdAsync(userId, ct);

        return await _db.CandidateAttributeValues
            .AsNoTracking()
            .Where(v => v.CandidateProfileId == profileId)
            .Include(v => v.AttributeDefinition)
            .OrderBy(v => v.AttributeDefinition.Category)
            .ThenBy(v => v.AttributeDefinition.Name)
            .Select(v => MapAttributeValue(v))
            .ToListAsync(ct);
    }

    public async Task<AttributeValueResponse> AddAttributeToProfileAsync(Guid userId, AddAttributeToProfileRequest request, CancellationToken ct = default)
    {
        var profileId = await GetProfileIdAsync(userId, ct);

        // Check attribute exists
        var attr = await _db.AttributeDefinitions
            .FirstOrDefaultAsync(a => a.Id == request.AttributeDefinitionId, ct)
            ?? throw new KeyNotFoundException("Attribute not found.");

        // Prevent duplicates
        var exists = await _db.CandidateAttributeValues
            .AnyAsync(v => v.CandidateProfileId == profileId && v.AttributeDefinitionId == request.AttributeDefinitionId, ct);
        if (exists)
            throw new InvalidOperationException("This attribute is already in your profile.");

        var value = new CandidateAttributeValue
        {
            CandidateProfileId = profileId,
            AttributeDefinitionId = request.AttributeDefinitionId
        };

        _db.CandidateAttributeValues.Add(value);

        // Track recently used
        await UpsertRecentlyUsedAsync(userId, request.AttributeDefinitionId, ct);

        await _db.SaveChangesAsync(ct);

        // Reload with navigation
        await _db.Entry(value).Reference(v => v.AttributeDefinition).LoadAsync(ct);
        return MapAttributeValue(value);
    }

    public async Task<AttributeValueResponse> UpdateAttributeValueAsync(Guid userId, Guid attributeValueId, UpdateAttributeValueRequest request, CancellationToken ct = default)
    {
        var profileId = await GetProfileIdAsync(userId, ct);

        var value = await _db.CandidateAttributeValues
            .Include(v => v.AttributeDefinition)
            .FirstOrDefaultAsync(v => v.Id == attributeValueId && v.CandidateProfileId == profileId, ct)
            ?? throw new KeyNotFoundException("Attribute value not found.");

        // Optimistic locking check
        var clientVersion = Convert.FromBase64String(request.RowVersion);
        if (!clientVersion.SequenceEqual(value.RowVersion))
            throw new Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException(
                "Attribute value was modified by another process. Please reload.", []);

        // Update only the field matching this attribute's data type
        value.StringValue = request.StringValue;
        value.TextValue = request.TextValue;
        value.ImageUrl = request.ImageUrl;
        value.NumericValue = request.NumericValue;
        value.DateValue = request.DateValue;
        value.DateEndValue = request.DateEndValue;
        value.BoolValue = request.BoolValue;
        value.OptionValue = request.OptionValue;
        value.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return MapAttributeValue(value);
    }

    public async Task RemoveAttributeFromProfileAsync(Guid userId, Guid attributeValueId, CancellationToken ct = default)
    {
        var profileId = await GetProfileIdAsync(userId, ct);

        var value = await _db.CandidateAttributeValues
            .FirstOrDefaultAsync(v => v.Id == attributeValueId && v.CandidateProfileId == profileId, ct)
            ?? throw new KeyNotFoundException("Attribute value not found.");

        _db.CandidateAttributeValues.Remove(value);
        await _db.SaveChangesAsync(ct);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<Guid> GetProfileIdAsync(Guid userId, CancellationToken ct)
    {
        var id = await _db.CandidateProfiles
            .Where(p => p.UserId == userId)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException("Profile not found.");
        return id;
    }

    private async Task UpsertRecentlyUsedAsync(Guid userId, Guid attributeId, CancellationToken ct)
    {
        var existing = await _db.RecentlyUsedAttributes
            .FirstOrDefaultAsync(r => r.UserId == userId && r.AttributeDefinitionId == attributeId, ct);

        if (existing is not null)
            existing.UsedAt = DateTime.UtcNow;
        else
            _db.RecentlyUsedAttributes.Add(new RecentlyUsedAttribute
            {
                UserId = userId,
                AttributeDefinitionId = attributeId
            });
    }

    private static ProfileResponse MapProfile(CandidateProfile p) => new(
        Id: p.Id,
        UserId: p.UserId,
        FirstName: p.FirstName,
        LastName: p.LastName,
        Location: p.Location,
        PhotoUrl: p.PhotoUrl,
        CreatedAt: p.CreatedAt,
        UpdatedAt: p.UpdatedAt,
        RowVersion: Convert.ToBase64String(p.RowVersion)
    );

    private static AttributeValueResponse MapAttributeValue(CandidateAttributeValue v) => new(
        Id: v.Id,
        AttributeDefinitionId: v.AttributeDefinitionId,
        AttributeName: v.AttributeDefinition.Name,
        AttributeCategory: v.AttributeDefinition.Category,
        DataType: v.AttributeDefinition.DataType,
        StringValue: v.StringValue,
        TextValue: v.TextValue,
        ImageUrl: v.ImageUrl,
        NumericValue: v.NumericValue,
        DateValue: v.DateValue,
        DateEndValue: v.DateEndValue,
        BoolValue: v.BoolValue,
        OptionValue: v.OptionValue,
        RowVersion: Convert.ToBase64String(v.RowVersion)
    );
}
