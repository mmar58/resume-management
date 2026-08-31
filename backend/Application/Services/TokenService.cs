using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using backend.Application.DTOs.Auth;
using backend.Data;
using backend.Domain.Entities;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace backend.Application.Services;

public class TokenService : ITokenService
{
    private readonly AppDbContext _db;
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessExpireMinutes;
    private readonly int _refreshExpireDays;

    public TokenService(AppDbContext db)
    {
        _db = db;
        _secret = Env.GetString("JWT_SECRET")
            ?? throw new InvalidOperationException("JWT_SECRET not configured.");
        _issuer = Env.GetString("JWT_ISSUER") ?? "resume-management-backend";
        _audience = Env.GetString("JWT_AUDIENCE") ?? "resume-management-frontend";
        _accessExpireMinutes = int.TryParse(Env.GetString("JWT_EXPIRES_MINUTES"), out var m) ? m : 60;
        _refreshExpireDays = int.TryParse(Env.GetString("JWT_REFRESH_EXPIRES_DAYS"), out var d) ? d : 7;
    }

    public string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Name, user.DisplayName ?? user.Email ?? ""),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessExpireMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GenerateRefreshTokenAsync(Guid userId, CancellationToken ct = default)
    {
        // Generate a cryptographically secure random token
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var hash = HashToken(rawToken);

        // Revoke any existing active tokens for this user (single-session per user)
        await _db.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.IsRevoked, true), ct);

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId,
            TokenHash = hash,
            ExpiresAt = DateTime.UtcNow.AddDays(_refreshExpireDays)
        });

        await _db.SaveChangesAsync(ct);
        return rawToken;
    }

    public async Task<User?> ValidateAndRevokeRefreshTokenAsync(string rawToken, CancellationToken ct = default)
    {
        var hash = HashToken(rawToken);

        var stored = await _db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

        if (stored is null || stored.IsRevoked || stored.ExpiresAt <= DateTime.UtcNow)
            return null;

        // Revoke the used token (rotate on each refresh)
        stored.IsRevoked = true;
        await _db.SaveChangesAsync(ct);

        return stored.User;
    }

    public async Task<AuthResponse> BuildAuthResponseAsync(User user, CancellationToken ct = default)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id, ct);
        var expiresAt = DateTime.UtcNow.AddMinutes(_accessExpireMinutes);

        return new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            ExpiresAt: expiresAt,
            User: new UserInfo(
                Id: user.Id,
                Email: user.Email ?? "",
                DisplayName: user.DisplayName,
                Role: user.Role.ToString(),
                PreferredLocale: user.PreferredLocale,
                PreferredTheme: user.PreferredTheme
            )
        );
    }

    private static string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
