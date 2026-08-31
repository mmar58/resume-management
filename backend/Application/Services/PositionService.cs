using backend.Application.DTOs.Common;
using backend.Application.DTOs.Positions;
using backend.Data;
using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Services;

public class PositionService : IPositionService
{
    private readonly AppDbContext _db;

    public PositionService(AppDbContext db) => _db = db;

    public async Task<PagedResponse<PositionSummaryResponse>> GetPositionsAsync(bool onlyActive = true, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var query = _db.Positions.AsNoTracking().AsQueryable();

        if (onlyActive)
        {
            query = query.Where(p => p.IsActive);
        }

        var totalCount = await query.CountAsync(ct);

        var positions = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(p => p.PositionAttributes)
            .Include(p => p.AccessRules)
            .ToListAsync(ct);

        var items = positions.Select(p => new PositionSummaryResponse(
            Id: p.Id,
            Title: p.Title,
            Company: p.Company,
            Level: p.Level,
            IsActive: p.IsActive,
            CreatedAt: p.CreatedAt,
            RequiredAttributesCount: p.PositionAttributes.Count,
            AccessRulesCount: p.AccessRules.Count
        )).ToList();

        return new PagedResponse<PositionSummaryResponse>(items, totalCount, page, pageSize);
    }

    public async Task<PositionResponse> GetPositionByIdAsync(Guid id, CancellationToken ct = default)
    {
        var position = await _db.Positions
            .Include(p => p.PositionAttributes)
                .ThenInclude(pa => pa.AttributeDefinition)
            .Include(p => p.ProjectTags)
            .Include(p => p.AccessRules)
                .ThenInclude(ar => ar.AttributeDefinition)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new KeyNotFoundException("Position not found.");

        return MapPosition(position);
    }

    public async Task<PositionResponse> CreatePositionAsync(CreatePositionRequest request, CancellationToken ct = default)
    {
        var position = new Position
        {
            Title = request.Title,
            ShortDescription = request.ShortDescription,
            Company = request.Company,
            Level = request.Level,
            IsActive = true
        };

        if (request.AttributeDefinitionIds?.Any() == true)
        {
            var distinctIds = request.AttributeDefinitionIds.Distinct().ToList();
            var validAttributes = await _db.AttributeDefinitions
                .Where(a => distinctIds.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id, ct);

            position.PositionAttributes = distinctIds
                .Where(validAttributes.ContainsKey)
                .Select(id => new PositionAttribute { AttributeDefinitionId = id })
                .ToList();
        }

        if (request.ProjectTags?.Any() == true)
        {
            position.ProjectTags = request.ProjectTags
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(t => new PositionProjectTag { Tag = t.Trim().ToLowerInvariant() })
                .ToList();
        }

        if (request.AccessRules?.Any() == true)
        {
             var ruleAttrIds = request.AccessRules.Select(r => r.AttributeDefinitionId).Distinct().ToList();
             var validRuleAttributes = await _db.AttributeDefinitions
                .Where(a => ruleAttrIds.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id, ct);

            position.AccessRules = request.AccessRules
                .Where(r => validRuleAttributes.ContainsKey(r.AttributeDefinitionId))
                .Select(r => new PositionAccessRule
                {
                    AttributeDefinitionId = r.AttributeDefinitionId,
                    Operator = r.Operator,
                    Value = r.Value
                })
                .ToList();
        }

        _db.Positions.Add(position);
        await _db.SaveChangesAsync(ct);
        
        // Reload with includes for mapping
        return await GetPositionByIdAsync(position.Id, ct);
    }

    public async Task<PositionResponse> DuplicatePositionAsync(Guid id, CancellationToken ct = default)
    {
        var existing = await _db.Positions
            .Include(p => p.PositionAttributes)
            .Include(p => p.ProjectTags)
            .Include(p => p.AccessRules)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new KeyNotFoundException("Position not found.");

        var duplicate = new Position
        {
            Title = existing.Title + " (Copy)",
            ShortDescription = existing.ShortDescription,
            Company = existing.Company,
            Level = existing.Level,
            IsActive = false, // Set duplicates to inactive by default
            PositionAttributes = existing.PositionAttributes.Select(pa => new PositionAttribute { AttributeDefinitionId = pa.AttributeDefinitionId }).ToList(),
            ProjectTags = existing.ProjectTags.Select(pt => new PositionProjectTag { Tag = pt.Tag }).ToList(),
            AccessRules = existing.AccessRules.Select(ar => new PositionAccessRule
            {
                AttributeDefinitionId = ar.AttributeDefinitionId,
                Operator = ar.Operator,
                Value = ar.Value
            }).ToList()
        };

        _db.Positions.Add(duplicate);
        await _db.SaveChangesAsync(ct);

        return await GetPositionByIdAsync(duplicate.Id, ct);
    }

    public async Task<PositionResponse> UpdatePositionAsync(Guid id, UpdatePositionRequest request, CancellationToken ct = default)
    {
        var position = await _db.Positions
            .Include(p => p.PositionAttributes)
            .Include(p => p.ProjectTags)
            .Include(p => p.AccessRules)
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new KeyNotFoundException("Position not found.");

        // Optimistic locking
        var clientVersion = Convert.FromBase64String(request.RowVersion);
        if (!clientVersion.SequenceEqual(position.RowVersion))
            throw new Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException(
                "Position was modified by another process. Please reload.", []);

        position.Title = request.Title;
        position.ShortDescription = request.ShortDescription;
        position.Company = request.Company;
        position.Level = request.Level;
        position.IsActive = request.IsActive;
        position.UpdatedAt = DateTime.UtcNow;

        // Update Attributes
        _db.PositionAttributes.RemoveRange(position.PositionAttributes);
        if (request.AttributeDefinitionIds?.Any() == true)
        {
            var distinctIds = request.AttributeDefinitionIds.Distinct().ToList();
            var validAttributes = await _db.AttributeDefinitions
                .Where(a => distinctIds.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id, ct);

            position.PositionAttributes = distinctIds
                .Where(validAttributes.ContainsKey)
                .Select(attrId => new PositionAttribute { PositionId = id, AttributeDefinitionId = attrId })
                .ToList();
        }
        else
        {
            position.PositionAttributes = new List<PositionAttribute>();
        }

        // Update Project Tags
        _db.PositionProjectTags.RemoveRange(position.ProjectTags);
        if (request.ProjectTags?.Any() == true)
        {
            position.ProjectTags = request.ProjectTags
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(t => new PositionProjectTag { PositionId = id, Tag = t.Trim().ToLowerInvariant() })
                .ToList();
        }
        else
        {
            position.ProjectTags = new List<PositionProjectTag>();
        }

        // Update Access Rules
        _db.PositionAccessRules.RemoveRange(position.AccessRules);
        if (request.AccessRules?.Any() == true)
        {
             var ruleAttrIds = request.AccessRules.Select(r => r.AttributeDefinitionId).Distinct().ToList();
             var validRuleAttributes = await _db.AttributeDefinitions
                .Where(a => ruleAttrIds.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id, ct);

            position.AccessRules = request.AccessRules
                .Where(r => validRuleAttributes.ContainsKey(r.AttributeDefinitionId))
                .Select(r => new PositionAccessRule
                {
                    PositionId = id,
                    AttributeDefinitionId = r.AttributeDefinitionId,
                    Operator = r.Operator,
                    Value = r.Value
                })
                .ToList();
        }
        else
        {
            position.AccessRules = new List<PositionAccessRule>();
        }

        await _db.SaveChangesAsync(ct);
        
        return await GetPositionByIdAsync(position.Id, ct);
    }

    public async Task DeletePositionAsync(Guid id, CancellationToken ct = default)
    {
        var position = await _db.Positions
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new KeyNotFoundException("Position not found.");

        _db.Positions.Remove(position);
        await _db.SaveChangesAsync(ct);
    }

    private static PositionResponse MapPosition(Position p) => new(
        Id: p.Id,
        Title: p.Title,
        ShortDescription: p.ShortDescription,
        Company: p.Company,
        Level: p.Level,
        IsActive: p.IsActive,
        CreatedAt: p.CreatedAt,
        UpdatedAt: p.UpdatedAt,
        RowVersion: Convert.ToBase64String(p.RowVersion),
        Attributes: p.PositionAttributes.Select(pa => new PositionAttributeResponse(
            Id: pa.Id,
            AttributeDefinitionId: pa.AttributeDefinitionId,
            AttributeName: pa.AttributeDefinition.Name,
            DataType: pa.AttributeDefinition.DataType
        )).ToList(),
        ProjectTags: p.ProjectTags.Select(pt => pt.Tag).ToList(),
        AccessRules: p.AccessRules.Select(ar => new PositionAccessRuleResponse(
            Id: ar.Id,
            AttributeDefinitionId: ar.AttributeDefinitionId,
            AttributeName: ar.AttributeDefinition.Name,
            Operator: ar.Operator,
            Value: ar.Value
        )).ToList()
    );
}
