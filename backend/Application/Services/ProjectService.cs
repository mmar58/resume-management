using backend.Application.DTOs.Projects;
using backend.Data;
using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Services;

public interface IProjectService
{
    Task<List<ProjectResponse>> GetProjectsAsync(Guid userId, CancellationToken ct = default);
    Task<ProjectResponse> CreateProjectAsync(Guid userId, CreateProjectRequest request, CancellationToken ct = default);
    Task<ProjectResponse> UpdateProjectAsync(Guid userId, Guid projectId, UpdateProjectRequest request, CancellationToken ct = default);
    Task DeleteProjectAsync(Guid userId, Guid projectId, CancellationToken ct = default);
    Task<List<string>> GetTagSuggestionsAsync(string prefix, CancellationToken ct = default);
}

public class ProjectService : IProjectService
{
    private readonly AppDbContext _db;

    public ProjectService(AppDbContext db) => _db = db;

    public async Task<List<ProjectResponse>> GetProjectsAsync(Guid userId, CancellationToken ct = default)
    {
        var profileId = await GetProfileIdAsync(userId, ct);

        return await _db.Projects
            .AsNoTracking()
            .Where(p => p.CandidateProfileId == profileId)
            .Include(p => p.Tags)
            .OrderByDescending(p => p.EndDate)
            .ThenByDescending(p => p.StartDate)
            .Select(p => MapProject(p))
            .ToListAsync(ct);
    }

    public async Task<ProjectResponse> CreateProjectAsync(Guid userId, CreateProjectRequest request, CancellationToken ct = default)
    {
        var profileId = await GetProfileIdAsync(userId, ct);

        var project = new Project
        {
            CandidateProfileId = profileId,
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Description = request.Description
        };

        if (request.Tags is not null)
            project.Tags = request.Tags
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(t => new ProjectTag { Tag = t.Trim().ToLowerInvariant() })
                .ToList();

        _db.Projects.Add(project);
        await _db.SaveChangesAsync(ct);
        return MapProject(project);
    }

    public async Task<ProjectResponse> UpdateProjectAsync(Guid userId, Guid projectId, UpdateProjectRequest request, CancellationToken ct = default)
    {
        var profileId = await GetProfileIdAsync(userId, ct);

        var project = await _db.Projects
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == projectId && p.CandidateProfileId == profileId, ct)
            ?? throw new KeyNotFoundException("Project not found.");

        // Optimistic locking
        var clientVersion = Convert.FromBase64String(request.RowVersion);
        if (!clientVersion.SequenceEqual(project.RowVersion))
            throw new Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException(
                "Project was modified by another process. Please reload.", []);

        project.Name = request.Name;
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;
        project.Description = request.Description;
        project.UpdatedAt = DateTime.UtcNow;

        // Replace tags
        _db.ProjectTags.RemoveRange(project.Tags);
        project.Tags = (request.Tags ?? [])
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(t => new ProjectTag { ProjectId = projectId, Tag = t.Trim().ToLowerInvariant() })
            .ToList();

        await _db.SaveChangesAsync(ct);
        return MapProject(project);
    }

    public async Task DeleteProjectAsync(Guid userId, Guid projectId, CancellationToken ct = default)
    {
        var profileId = await GetProfileIdAsync(userId, ct);

        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.CandidateProfileId == profileId, ct)
            ?? throw new KeyNotFoundException("Project not found.");

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Returns up to 10 distinct tags that start with the given prefix.
    /// Used for autocomplete (Section 9 / tech tag feature).
    /// </summary>
    public async Task<List<string>> GetTagSuggestionsAsync(string prefix, CancellationToken ct = default)
    {
        return await _db.ProjectTags
            .AsNoTracking()
            .Where(t => t.Tag.StartsWith(prefix.ToLowerInvariant()))
            .Select(t => t.Tag)
            .Distinct()
            .OrderBy(t => t)
            .Take(10)
            .ToListAsync(ct);
    }

    private async Task<Guid> GetProfileIdAsync(Guid userId, CancellationToken ct)
    {
        var id = await _db.CandidateProfiles
            .Where(p => p.UserId == userId)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException("Profile not found.");
        return id;
    }

    private static ProjectResponse MapProject(Project p) => new(
        Id: p.Id,
        Name: p.Name,
        StartDate: p.StartDate,
        EndDate: p.EndDate,
        Description: p.Description,
        Tags: p.Tags.Select(t => t.Tag).OrderBy(t => t).ToList(),
        CreatedAt: p.CreatedAt,
        UpdatedAt: p.UpdatedAt,
        RowVersion: Convert.ToBase64String(p.RowVersion)
    );
}
