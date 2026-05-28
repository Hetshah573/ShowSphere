namespace ShowSphere.Application.Features.Users.DTOs;

public record UserProfileDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? Phone,
    string Role,
    DateTime CreatedAt,
    int TotalBookings,
    int TotalReviews);

public record UpdateProfileRequest(string FirstName, string LastName, string? Phone);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record ForgotPasswordRequest(string Email);

public record ResetPasswordRequest(string Token, string NewPassword);
