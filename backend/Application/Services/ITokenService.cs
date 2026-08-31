using backend.Application.DTOs.Auth;
using backend.Domain.Entities;

namespace backend.Application.Services;

public interface ITokenService
{
    /// <summary>Generates a signed JWT access token for the given user.</summary>
    string GenerateAccessToken(User user);

    /// <summary>Generates a cryptographically secure refresh token, persists its hash, and returns the raw value.</summary>
    Task<string> GenerateRefreshTokenAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Validates a raw refresh token. Returns the associated user if valid,
    /// revokes the token, and returns null if invalid/expired/revoked.
    /// </summary>
    Task<User?> ValidateAndRevokeRefreshTokenAsync(string rawToken, CancellationToken ct = default);

    /// <summary>Builds the full AuthResponse DTO from a user entity.</summary>
    Task<AuthResponse> BuildAuthResponseAsync(User user, CancellationToken ct = default);
}
