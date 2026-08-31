using backend.Application.DTOs.Auth;
using backend.Data;
using backend.Domain.Entities;
using backend.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _db;
    private readonly ITokenService _tokenService;

    public AuthService(UserManager<User> userManager, AppDbContext db, ITokenService tokenService)
    {
        _userManager = userManager;
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            throw new InvalidOperationException("An account with this email already exists.");

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            Role = UserRole.Candidate
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, UserRole.Candidate.ToString());

        // Create the candidate profile automatically
        await CreateProfileForUserAsync(user, ct);

        return await _tokenService.BuildAuthResponseAsync(user, ct);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (user.IsBlocked)
            throw new UnauthorizedAccessException("Your account has been blocked.");

        var valid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!valid)
            throw new UnauthorizedAccessException("Invalid email or password.");

        // Update last seen
        user.LastSeenAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return await _tokenService.BuildAuthResponseAsync(user, ct);
    }

    public async Task<AuthResponse> RefreshAsync(string rawRefreshToken, CancellationToken ct = default)
    {
        var user = await _tokenService.ValidateAndRevokeRefreshTokenAsync(rawRefreshToken, ct)
            ?? throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        if (user.IsBlocked)
            throw new UnauthorizedAccessException("Your account has been blocked.");

        return await _tokenService.BuildAuthResponseAsync(user, ct);
    }

    public async Task LogoutAsync(Guid userId, CancellationToken ct = default)
    {
        // Revoke all active refresh tokens for this user
        await _db.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.IsRevoked, true), ct);
    }

    public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new KeyNotFoundException("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    public async Task UpdatePreferencesAsync(Guid userId, UpdatePreferencesRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users.FindAsync([userId], ct)
            ?? throw new KeyNotFoundException("User not found.");

        if (request.PreferredLocale is not null) user.PreferredLocale = request.PreferredLocale;
        if (request.PreferredTheme is not null) user.PreferredTheme = request.PreferredTheme;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<AuthResponse> HandleOAuthCallbackAsync(
        string providerEmail,
        string providerKey,
        string? displayName,
        OAuthProvider provider,
        CancellationToken ct = default)
    {
        // Check if this social account is already linked
        var social = await _db.SocialAccounts
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Provider == provider && s.ProviderKey == providerKey, ct);

        User user;
        if (social is not null)
        {
            user = social.User;
        }
        else
        {
            // Try to find existing user by email
            user = await _userManager.FindByEmailAsync(providerEmail)
                ?? await CreateOAuthUserAsync(providerEmail, displayName, ct);

            // Link social account
            _db.SocialAccounts.Add(new SocialAccount
            {
                UserId = user.Id,
                Provider = provider,
                ProviderKey = providerKey,
                Email = providerEmail
            });
            await _db.SaveChangesAsync(ct);
        }

        if (user.IsBlocked)
            throw new UnauthorizedAccessException("Your account has been blocked.");

        user.LastSeenAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return await _tokenService.BuildAuthResponseAsync(user, ct);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<User> CreateOAuthUserAsync(string email, string? displayName, CancellationToken ct)
    {
        var user = new User
        {
            UserName = email,
            Email = email,
            DisplayName = displayName,
            Role = UserRole.Candidate,
            EmailConfirmed = true  // OAuth email is already verified by provider
        };

        // OAuth users have no local password — use a random one so Identity is satisfied
        var result = await _userManager.CreateAsync(user, Guid.NewGuid().ToString("N") + "Aa1!");
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, UserRole.Candidate.ToString());
        await CreateProfileForUserAsync(user, ct);
        return user;
    }

    private async Task CreateProfileForUserAsync(User user, CancellationToken ct)
    {
        // Parse display name into first/last name if available
        string? firstName = null;
        string? lastName = null;
        if (!string.IsNullOrWhiteSpace(user.DisplayName))
        {
            var parts = user.DisplayName.Trim().Split(' ', 2);
            firstName = parts[0];
            lastName = parts.Length > 1 ? parts[1] : null;
        }

        _db.CandidateProfiles.Add(new CandidateProfile
        {
            UserId = user.Id,
            FirstName = firstName,
            LastName = lastName
        });
        await _db.SaveChangesAsync(ct);
    }
}
