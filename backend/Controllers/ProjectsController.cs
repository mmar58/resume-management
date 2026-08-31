using backend.Application.DTOs.Projects;
using backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

/// <summary>
/// Candidate's personal project library.
/// All endpoints are scoped to the authenticated candidate.
/// </summary>
[Authorize]
public class ProjectsController : ApiControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService) => _projectService = projectService;

    // GET /api/projects
    [HttpGet]
    [ProducesResponseType(typeof(List<ProjectResponse>), 200)]
    public async Task<IActionResult> GetProjects(CancellationToken ct)
    {
        var projects = await _projectService.GetProjectsAsync(CurrentUserId, ct);
        return Ok(projects);
    }

    // POST /api/projects
    [HttpPost]
    [ProducesResponseType(typeof(ProjectResponse), 201)]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request, CancellationToken ct)
    {
        var project = await _projectService.CreateProjectAsync(CurrentUserId, request, ct);
        return StatusCode(201, project);
    }

    // PUT /api/projects/{id}
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProjectResponse), 200)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request, CancellationToken ct)
    {
        var project = await _projectService.UpdateProjectAsync(CurrentUserId, id, request, ct);
        return Ok(project);
    }

    // DELETE /api/projects/{id}
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteProject(Guid id, CancellationToken ct)
    {
        await _projectService.DeleteProjectAsync(CurrentUserId, id, ct);
        return NoContent();
    }

    // GET /api/projects/tags/suggestions?prefix=react
    [HttpGet("tags/suggestions")]
    [ProducesResponseType(typeof(List<string>), 200)]
    public async Task<IActionResult> GetTagSuggestions([FromQuery] string prefix = "", CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(prefix) || prefix.Length < 1)
            return Ok(Array.Empty<string>());

        var tags = await _projectService.GetTagSuggestionsAsync(prefix, ct);
        return Ok(tags);
    }
}
