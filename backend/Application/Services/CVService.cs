using backend.Application.DTOs.Common;
using backend.Application.DTOs.CVs;
using backend.Application.DTOs.Profile;
using backend.Application.DTOs.Projects;
using backend.Data;
using backend.Domain.Entities;
using backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Services;

public class CVService : ICVService
{
    private readonly AppDbContext _db;
    private readonly IAccessRuleEvaluationService _evaluationService;

    public CVService(AppDbContext db, IAccessRuleEvaluationService evaluationService)
    {
        _db = db;
        _evaluationService = evaluationService;
    }

    // ── Candidate Operations ──────────────────────────────────────────────────

    public async Task<CVResponse> CreateCVAsync(Guid userId, CreateCVRequest request, CancellationToken ct = default)
    {
        var profileId = await GetProfileIdAsync(userId, ct);

        // Check if CV already exists
        var exists = await _db.CVs.AnyAsync(cv => cv.CandidateProfileId == profileId && cv.PositionId == request.PositionId, ct);
        if (exists)
            throw new InvalidOperationException("You already have a CV for this position.");

        // Check Position Activity
        var position = await _db.Positions
            .Include(p => p.AccessRules)
                .ThenInclude(ar => ar.AttributeDefinition)
            .FirstOrDefaultAsync(p => p.Id == request.PositionId, ct)
            ?? throw new KeyNotFoundException("Position not found.");

        if (!position.IsActive)
            throw new InvalidOperationException("This position is no longer active.");

        // Fetch candidate's ALL attribute values for rule evaluation
        var allCandidateValues = await _db.CandidateAttributeValues
            .Include(cav => cav.AttributeDefinition)
            .Where(cav => cav.CandidateProfileId == profileId)
            .ToListAsync(ct);

        // Evaluate rules
        if (!_evaluationService.Evaluate(position.AccessRules.ToList(), allCandidateValues))
        {
            throw new InvalidOperationException("You do not meet the requirements to apply for this position.");
        }

        var cv = new CV
        {
            CandidateProfileId = profileId,
            PositionId = request.PositionId,
            Status = CVStatus.Draft
        };

        // Add selected attributes
        if (request.SelectedAttributeValueIds?.Any() == true)
        {
            var validIds = request.SelectedAttributeValueIds.Intersect(allCandidateValues.Select(v => v.Id)).Distinct();
            cv.SelectedAttributes = validIds.Select(id => new CVAttributeValue { CandidateAttributeValueId = id }).ToList();
        }

        // Add selected projects
        if (request.SelectedProjectIds?.Any() == true)
        {
            var validIds = await _db.Projects
                .Where(p => p.CandidateProfileId == profileId && request.SelectedProjectIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync(ct);

            cv.SelectedProjects = validIds.Select(id => new CVProject { ProjectId = id }).ToList();
        }

        _db.CVs.Add(cv);
        await _db.SaveChangesAsync(ct);

        return await GetCandidateCVAsync(userId, cv.Id, ct);
    }

    public async Task<CVResponse> UpdateCVAsync(Guid userId, Guid cvId, UpdateCVRequest request, CancellationToken ct = default)
    {
        var profileId = await GetProfileIdAsync(userId, ct);

        var cv = await _db.CVs
            .Include(c => c.SelectedAttributes)
            .Include(c => c.SelectedProjects)
            .FirstOrDefaultAsync(c => c.Id == cvId && c.CandidateProfileId == profileId, ct)
            ?? throw new KeyNotFoundException("CV not found.");

        if (cv.Status == CVStatus.Published)
            throw new InvalidOperationException("Cannot edit a submitted CV. Withdraw it first.");

        // Optimistic locking
        var clientVersion = Convert.FromBase64String(request.RowVersion);
        if (!clientVersion.SequenceEqual(cv.RowVersion))
            throw new DbUpdateConcurrencyException("CV was modified by another process. Please reload.", []);

        cv.UpdatedAt = DateTime.UtcNow;

        // Update selected attributes
        _db.CVAttributeValues.RemoveRange(cv.SelectedAttributes);
        if (request.SelectedAttributeValueIds?.Any() == true)
        {
            var allCandidateValues = await _db.CandidateAttributeValues
                .Where(cav => cav.CandidateProfileId == profileId)
                .Select(cav => cav.Id)
                .ToListAsync(ct);

            var validIds = request.SelectedAttributeValueIds.Intersect(allCandidateValues).Distinct();
            cv.SelectedAttributes = validIds.Select(id => new CVAttributeValue { CVId = cvId, CandidateAttributeValueId = id }).ToList();
        }
        else
        {
             cv.SelectedAttributes = new List<CVAttributeValue>();
        }

        // Update selected projects
        _db.CVProjects.RemoveRange(cv.SelectedProjects);
        if (request.SelectedProjectIds?.Any() == true)
        {
            var allCandidateProjects = await _db.Projects
                .Where(p => p.CandidateProfileId == profileId)
                .Select(p => p.Id)
                .ToListAsync(ct);

            var validIds = request.SelectedProjectIds.Intersect(allCandidateProjects).Distinct();
            cv.SelectedProjects = validIds.Select(id => new CVProject { CVId = cvId, ProjectId = id }).ToList();
        }
        else
        {
            cv.SelectedProjects = new List<CVProject>();
        }

        await _db.SaveChangesAsync(ct);
        return await GetCandidateCVAsync(userId, cvId, ct);
    }

    public async Task<CVResponse> GetCandidateCVAsync(Guid userId, Guid cvId, CancellationToken ct = default)
    {
        var profileId = await GetProfileIdAsync(userId, ct);

        var cv = await _db.CVs
            .Include(c => c.Position)
            .Include(c => c.CandidateProfile)
            .Include(c => c.Likes)
            .Include(c => c.SelectedAttributes)
                .ThenInclude(sa => sa.CandidateAttributeValue)
                    .ThenInclude(cav => cav.AttributeDefinition)
            .Include(c => c.SelectedProjects)
                .ThenInclude(sp => sp.Project)
                    .ThenInclude(p => p.Tags)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == cvId && c.CandidateProfileId == profileId, ct)
            ?? throw new KeyNotFoundException("CV not found.");

        return MapCVResponse(cv, userId); // Use userId to check if they liked it (unlikely for candidates, but consistent)
    }

    public async Task<PagedResponse<CVSummaryResponse>> GetCandidateCVsAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var profileId = await GetProfileIdAsync(userId, ct);

        var query = _db.CVs
            .Include(c => c.Position)
            .Include(c => c.CandidateProfile)
            .Include(c => c.Likes)
            .Where(c => c.CandidateProfileId == profileId)
            .AsNoTracking();

        var totalCount = await query.CountAsync(ct);

        var cvs = await query
            .OrderByDescending(c => c.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = cvs.Select(c => MapCVSummary(c, userId)).ToList();
        return new PagedResponse<CVSummaryResponse>(items, totalCount, page, pageSize);
    }

    public async Task<CVResponse> ChangeCVStatusAsync(Guid userId, Guid cvId, ChangeCVStatusRequest request, CancellationToken ct = default)
    {
        var profileId = await GetProfileIdAsync(userId, ct);

        var cv = await _db.CVs
            .Include(c => c.Position)
                .ThenInclude(p => p.AccessRules)
            .FirstOrDefaultAsync(c => c.Id == cvId && c.CandidateProfileId == profileId, ct)
            ?? throw new KeyNotFoundException("CV not found.");

        var clientVersion = Convert.FromBase64String(request.RowVersion);
        if (!clientVersion.SequenceEqual(cv.RowVersion))
            throw new DbUpdateConcurrencyException("CV was modified by another process. Please reload.", []);

        if (request.Status == CVStatus.Published)
        {
            if (!cv.Position.IsActive)
                throw new InvalidOperationException("Cannot submit a CV to an inactive position.");

             // Re-evaluate rules on submission just in case they changed
             var allCandidateValues = await _db.CandidateAttributeValues
                .Include(cav => cav.AttributeDefinition)
                .Where(cav => cav.CandidateProfileId == profileId)
                .ToListAsync(ct);

             if (!_evaluationService.Evaluate(cv.Position.AccessRules.ToList(), allCandidateValues))
             {
                 throw new InvalidOperationException("You no longer meet the requirements to apply for this position.");
             }
        }

        cv.Status = request.Status;
        cv.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return await GetCandidateCVAsync(userId, cvId, ct);
    }

    // ── Recruiter Operations ──────────────────────────────────────────────────

    public async Task<PagedResponse<CVSummaryResponse>> GetSubmittedCVsAsync(Guid recruiterId, Guid? positionId = null, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var query = _db.CVs
            .Include(c => c.Position)
            .Include(c => c.CandidateProfile)
            .Include(c => c.Likes)
            .Where(c => c.Status == CVStatus.Published)
            .AsNoTracking();

        if (positionId.HasValue)
        {
            query = query.Where(c => c.PositionId == positionId.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var cvs = await query
            .OrderByDescending(c => c.UpdatedAt) // Most recently submitted first
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = cvs.Select(c => MapCVSummary(c, recruiterId)).ToList();
        return new PagedResponse<CVSummaryResponse>(items, totalCount, page, pageSize);
    }

    public async Task<CVResponse> GetCVForReviewAsync(Guid recruiterId, Guid cvId, CancellationToken ct = default)
    {
        var cv = await _db.CVs
            .Include(c => c.Position)
            .Include(c => c.CandidateProfile)
            .Include(c => c.Likes)
            .Include(c => c.SelectedAttributes)
                .ThenInclude(sa => sa.CandidateAttributeValue)
                    .ThenInclude(cav => cav.AttributeDefinition)
            .Include(c => c.SelectedProjects)
                .ThenInclude(sp => sp.Project)
                    .ThenInclude(p => p.Tags)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == cvId && c.Status == CVStatus.Published, ct)
            ?? throw new KeyNotFoundException("Published CV not found.");

        return MapCVResponse(cv, recruiterId);
    }

    public async Task ToggleCVLikeAsync(Guid recruiterId, Guid cvId, CancellationToken ct = default)
    {
        var cv = await _db.CVs
            .Include(c => c.Likes)
            .FirstOrDefaultAsync(c => c.Id == cvId && c.Status == CVStatus.Published, ct)
            ?? throw new KeyNotFoundException("Published CV not found.");

        var existingLike = cv.Likes.FirstOrDefault(l => l.RecruiterId == recruiterId);
        
        if (existingLike != null)
        {
            _db.CVLikes.Remove(existingLike);
        }
        else
        {
            _db.CVLikes.Add(new CVLike { CVId = cvId, RecruiterId = recruiterId });
        }

        await _db.SaveChangesAsync(ct);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<Guid> GetProfileIdAsync(Guid userId, CancellationToken ct)
    {
        var id = await _db.CandidateProfiles
            .Where(p => p.UserId == userId)
            .Select(p => p.Id)
            .FirstOrDefaultAsync(ct);
            
        if (id == Guid.Empty)
            throw new KeyNotFoundException("Candidate profile not found.");
            
        return id;
    }

    private static CVSummaryResponse MapCVSummary(CV c, Guid userId)
    {
        var name = (c.CandidateProfile.FirstName + " " + c.CandidateProfile.LastName).Trim();
        if (string.IsNullOrEmpty(name)) name = "Unknown Candidate";

        return new CVSummaryResponse(
            Id: c.Id,
            CandidateProfileId: c.CandidateProfileId,
            PositionId: c.PositionId,
            PositionTitle: c.Position?.Title ?? "Unknown Position",
            CandidateName: name,
            Status: c.Status,
            CreatedAt: c.CreatedAt,
            UpdatedAt: c.UpdatedAt,
            LikeCount: c.Likes.Count,
            HasLiked: c.Likes.Any(l => l.RecruiterId == userId)
        );
    }

    private static CVResponse MapCVResponse(CV c, Guid userId)
    {
        var name = (c.CandidateProfile.FirstName + " " + c.CandidateProfile.LastName).Trim();
        if (string.IsNullOrEmpty(name)) name = "Unknown Candidate";

        return new CVResponse(
            Id: c.Id,
            CandidateProfileId: c.CandidateProfileId,
            PositionId: c.PositionId,
            PositionTitle: c.Position?.Title ?? "Unknown Position",
            CandidateName: name,
            CandidatePhotoUrl: c.CandidateProfile.PhotoUrl,
            CandidateLocation: c.CandidateProfile.Location,
            Status: c.Status,
            CreatedAt: c.CreatedAt,
            UpdatedAt: c.UpdatedAt,
            LikeCount: c.Likes.Count,
            HasLiked: c.Likes.Any(l => l.RecruiterId == userId),
            RowVersion: Convert.ToBase64String(c.RowVersion),
            SelectedAttributes: c.SelectedAttributes
                .Select(sa => sa.CandidateAttributeValue)
                .OrderBy(v => v.AttributeDefinition.Category)
                .ThenBy(v => v.AttributeDefinition.Name)
                .Select(v => new AttributeValueResponse(
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
                )).ToList(),
            SelectedProjects: c.SelectedProjects
                .Select(sp => sp.Project)
                .OrderByDescending(p => p.EndDate)
                .ThenByDescending(p => p.StartDate)
                .Select(p => new ProjectResponse(
                    Id: p.Id,
                    Name: p.Name,
                    StartDate: p.StartDate,
                    EndDate: p.EndDate,
                    Description: p.Description,
                    Tags: p.Tags.Select(t => t.Tag).OrderBy(t => t).ToList(),
                    CreatedAt: p.CreatedAt,
                    UpdatedAt: p.UpdatedAt,
                    RowVersion: Convert.ToBase64String(p.RowVersion)
                )).ToList()
        );
    }
}
