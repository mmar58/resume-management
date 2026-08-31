namespace backend.Application.DTOs.Auth;

public record RegisterRequest(
    string Email,
    string Password,
    string DisplayName
);

public record LoginRequest(
    string Email,
    string Password
);

public record RefreshTokenRequest(
    string RefreshToken
);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserInfo User
);

public record UserInfo(
    Guid Id,
    string Email,
    string? DisplayName,
    string Role,
    string? PreferredLocale,
    string? PreferredTheme
);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);

public record UpdatePreferencesRequest(
    string? PreferredLocale,
    string? PreferredTheme
);
