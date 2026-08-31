using backend.Data;
using backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Middleware;

/// <summary>
/// Rejects requests from blocked users after JWT validation.
/// Blocked status is checked against the DB, not embedded in the token,
/// so blocks take effect immediately without waiting for token expiry.
/// </summary>
public class UserStatusCheckMiddleware
{
    private readonly RequestDelegate _next;

    public UserStatusCheckMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                var isBlocked = await db.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.IsBlocked)
                    .FirstOrDefaultAsync();

                if (isBlocked)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new { error = "Your account has been blocked." });
                    return;
                }
            }
        }

        await _next(context);
    }
}

public static class UserStatusCheckMiddlewareExtensions
{
    public static IApplicationBuilder UseUserStatusCheck(this IApplicationBuilder app)
        => app.UseMiddleware<UserStatusCheckMiddleware>();
}
