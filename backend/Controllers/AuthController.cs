using backend.Application.DTOs.Auth;
using backend.Application.Services;
using backend.Domain.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers;

/// <summary>
/// Handles registration, login, OAuth, token refresh, and logout.
/// All business logic is delegated to IAuthService.
/// </summary>
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _config;

    public AuthController(IAuthService authService, IConfiguration config)
    {
        _authService = authService;
        _config = config;
    }

    // POST /api/auth/register
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var response = await _authService.RegisterAsync(request, ct);
        return StatusCode(201, response);
    }

    // POST /api/auth/login
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var response = await _authService.LoginAsync(request, ct);
        return Ok(response);
    }

    // POST /api/auth/refresh
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var response = await _authService.RefreshAsync(request.RefreshToken, ct);
        return Ok(response);
    }

    // POST /api/auth/logout
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await _authService.LogoutAsync(CurrentUserId, ct);
        return NoContent();
    }

    // POST /api/auth/change-password
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        await _authService.ChangePasswordAsync(CurrentUserId, request.CurrentPassword, request.NewPassword, ct);
        return NoContent();
    }

    // PUT /api/auth/preferences
    [HttpPut("preferences")]
    [Authorize]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesRequest request, CancellationToken ct)
    {
        await _authService.UpdatePreferencesAsync(CurrentUserId, request, ct);
        return NoContent();
    }

    // ── OAuth — Google ────────────────────────────────────────────────────────

    // GET /api/auth/google/login
    [HttpGet("google/login")]
    [AllowAnonymous]
    public IActionResult GoogleLogin([FromQuery] string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(GoogleCallback), "Auth", new { returnUrl });
        var props = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(props, "Google");
    }

    // GET /api/auth/google/callback
    [HttpGet("google/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleCallback([FromQuery] string? returnUrl = null, CancellationToken ct = default)
    {
        return await HandleOAuthCallbackAsync("Google", OAuthProvider.Google, returnUrl, ct);
    }

    // ── OAuth — GitHub ────────────────────────────────────────────────────────

    // GET /api/auth/github/login
    [HttpGet("github/login")]
    [AllowAnonymous]
    public IActionResult GitHubLogin([FromQuery] string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(GitHubCallback), "Auth", new { returnUrl });
        var props = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(props, "GitHub");
    }

    // GET /api/auth/github/callback
    [HttpGet("github/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> GitHubCallback([FromQuery] string? returnUrl = null, CancellationToken ct = default)
    {
        return await HandleOAuthCallbackAsync("GitHub", OAuthProvider.GitHub, returnUrl, ct);
    }

    // ── Shared OAuth handler ──────────────────────────────────────────────────

    private async Task<IActionResult> HandleOAuthCallbackAsync(
        string scheme, OAuthProvider provider, string? returnUrl, CancellationToken ct)
    {
        var result = await HttpContext.AuthenticateAsync(scheme);
        if (!result.Succeeded)
            return BadRequest("OAuth authentication failed.");

        var email = result.Principal?.FindFirstValue(ClaimTypes.Email)
            ?? result.Principal?.FindFirstValue("email");
        var providerKey = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var displayName = result.Principal?.FindFirstValue(ClaimTypes.Name)
            ?? result.Principal?.FindFirstValue("name");

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(providerKey))
            return BadRequest("Could not retrieve email from OAuth provider.");

        var authResponse = await _authService.HandleOAuthCallbackAsync(email, providerKey, displayName, provider, ct);

        // Redirect to frontend with tokens in URL fragment (SPA picks them up)
        var frontendBase = _config["APP_ORIGINS"]?.Split(',').FirstOrDefault() ?? "http://localhost:5173";
        var redirect = $"{frontendBase}/auth/callback" +
                       $"#access_token={Uri.EscapeDataString(authResponse.AccessToken)}" +
                       $"&refresh_token={Uri.EscapeDataString(authResponse.RefreshToken)}" +
                       $"&expires_at={authResponse.ExpiresAt:O}";

        return Redirect(redirect);
    }
}
