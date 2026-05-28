namespace ShowSphere.Application.Features.Auth.DTOs;

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Phone);

public record LoginRequest(
    string Email,
    string Password);

public record RefreshTokenRequest(
    string RefreshToken);

public record GoogleLoginRequest(
    string IdToken);

public record AuthResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string? Phone,
    string Role,
    string AccessToken,
    string RefreshToken);

public record UserProfileDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? Phone,
    string Role,
    DateTime CreatedAt);
