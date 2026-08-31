using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers;

/// <summary>
/// Base class for all API controllers. Provides helpers for accessing
/// the current authenticated user's ID and role.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Returns the current user's ID, or throws if unauthenticated.</summary>
    protected Guid CurrentUserId
    {
        get
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User is not authenticated.");
            return Guid.Parse(claim);
        }
    }

    /// <summary>Returns the current user's role claim value.</summary>
    protected string? CurrentUserRole => User.FindFirstValue(ClaimTypes.Role);

    /// <summary>Returns true if the current user has the given role.</summary>
    protected bool IsInRole(string role) => User.IsInRole(role);
}
