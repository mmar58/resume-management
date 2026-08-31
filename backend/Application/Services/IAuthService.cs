using backend.Application.DTOs.Auth;
using backend.Domain.Enums;

namespace backend.Application.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResponse> RefreshAsync(string rawRefreshToken, CancellationToken ct = default);
    Task LogoutAsync(Guid userId, CancellationToken ct = default);
    Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken ct = default);
    Task UpdatePreferencesAsync(Guid userId, UpdatePreferencesRequest request, CancellationToken ct = default);

    /// <summary>
    /// Upserts a user from an OAuth provider callback.
    /// Creates account + profile if new user; links SocialAccount if existing.
    /// Returns a full AuthResponse.
    /// </summary>
    Task<AuthResponse> HandleOAuthCallbackAsync(
        string providerEmail,
        string providerKey,
        string? displayName,
        OAuthProvider provider,
        CancellationToken ct = default);
}
