using backend.Application.DTOs.Attributes;
using backend.Data;
using backend.Domain.Entities;
using backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Services;

public interface IAttributeService
{
    Task<List<AttributeResponse>> GetAttributesAsync(string? search = null, string? category = null, CancellationToken ct = default);
    Task<AttributeResponse> GetAttributeByIdAsync(Guid id, CancellationToken ct = default);
    Task<AttributeResponse> CreateAttributeAsync(CreateAttributeRequest request, CancellationToken ct = default);
    Task<AttributeResponse> UpdateAttributeAsync(Guid id, UpdateAttributeRequest request, CancellationToken ct = default);
    Task DeleteAttributeAsync(Guid id, CancellationToken ct = default);
    Task<List<AttributeResponse>> GetRecentlyUsedAttributesAsync(Guid userId, CancellationToken ct = default);
    Task<List<string>> GetCategoriesAsync(CancellationToken ct = default);
}

public class AttributeService : IAttributeService
{
    private readonly AppDbContext _db;

    public AttributeService(AppDbContext db) => _db = db;

    public async Task<List<AttributeResponse>> GetAttributesAsync(string? search = null, string? category = null, CancellationToken ct = default)
    {
        var query = _db.AttributeDefinitions.Include(a => a.Options).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLowerInvariant();
            query = query.Where(a => a.Name.ToLower().Contains(searchLower));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var categoryLower = category.ToLowerInvariant();
            query = query.Where(a => a.Category != null && a.Category.ToLower() == categoryLower);
        }

        var attributes = await query
            .OrderBy(a => a.Category)
            .ThenBy(a => a.Name)
            .ToListAsync(ct);

        return attributes.Select(MapAttribute).ToList();
    }

    public async Task<AttributeResponse> GetAttributeByIdAsync(Guid id, CancellationToken ct = default)
    {
        var attribute = await _db.AttributeDefinitions
            .Include(a => a.Options)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new KeyNotFoundException("Attribute not found.");

        return MapAttribute(attribute);
    }

    public async Task<AttributeResponse> CreateAttributeAsync(CreateAttributeRequest request, CancellationToken ct = default)
    {
        var exists = await _db.AttributeDefinitions
            .IgnoreQueryFilters() // check even soft-deleted ones
            .AnyAsync(a => a.Name.ToLower() == request.Name.ToLowerInvariant(), ct);

        if (exists)
            throw new InvalidOperationException($"An attribute with the name '{request.Name}' already exists.");

        var attribute = new AttributeDefinition
        {
            Name = request.Name,
            Category = request.Category,
            Description = request.Description,
            DataType = request.DataType
        };

        if (request.DataType == AttributeDataType.OneOfMany && request.Options?.Any() == true)
        {
            attribute.Options = request.Options
                .Where(o => !string.IsNullOrWhiteSpace(o))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(o => new AttributeOption { Value = o.Trim() })
                .ToList();
        }

        _db.AttributeDefinitions.Add(attribute);
        await _db.SaveChangesAsync(ct);
        return MapAttribute(attribute);
    }

    public async Task<AttributeResponse> UpdateAttributeAsync(Guid id, UpdateAttributeRequest request, CancellationToken ct = default)
    {
        var attribute = await _db.AttributeDefinitions
            .Include(a => a.Options)
            .FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new KeyNotFoundException("Attribute not found.");

        if (!string.Equals(attribute.Name, request.Name, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _db.AttributeDefinitions
                .IgnoreQueryFilters()
                .AnyAsync(a => a.Id != id && a.Name.ToLower() == request.Name.ToLowerInvariant(), ct);

            if (exists)
                throw new InvalidOperationException($"An attribute with the name '{request.Name}' already exists.");
        }

        attribute.Name = request.Name;
        attribute.Category = request.Category;
        attribute.Description = request.Description;
        attribute.UpdatedAt = DateTime.UtcNow;

        if (attribute.DataType == AttributeDataType.OneOfMany)
        {
            _db.AttributeOptions.RemoveRange(attribute.Options);
            attribute.Options = (request.Options ?? [])
                .Where(o => !string.IsNullOrWhiteSpace(o))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(o => new AttributeOption { AttributeDefinitionId = id, Value = o.Trim() })
                .ToList();
        }

        await _db.SaveChangesAsync(ct);
        return MapAttribute(attribute);
    }

    public async Task DeleteAttributeAsync(Guid id, CancellationToken ct = default)
    {
        var attribute = await _db.AttributeDefinitions
            .FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new KeyNotFoundException("Attribute not found.");

        attribute.IsDeleted = true; // Soft delete
        attribute.UpdatedAt = DateTime.UtcNow;
        
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<AttributeResponse>> GetRecentlyUsedAttributesAsync(Guid userId, CancellationToken ct = default)
    {
        var attributes = await _db.RecentlyUsedAttributes
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.UsedAt)
            .Take(10)
            .Select(r => r.AttributeDefinition)
            .Include(a => a.Options)
            .ToListAsync(ct);

        return attributes.Select(MapAttribute).ToList();
    }
    
    public async Task<List<string>> GetCategoriesAsync(CancellationToken ct = default)
    {
         return await _db.AttributeDefinitions
            .AsNoTracking()
            .Where(a => a.Category != null && a.Category != "")
            .Select(a => a.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(ct);
    }

    private static AttributeResponse MapAttribute(AttributeDefinition a) => new(
        Id: a.Id,
        Name: a.Name,
        Category: a.Category,
        Description: a.Description,
        DataType: a.DataType,
        Options: a.Options.Select(o => o.Value).OrderBy(v => v).ToList(),
        IsDeleted: a.IsDeleted,
        CreatedAt: a.CreatedAt,
        UpdatedAt: a.UpdatedAt
    );
}
