using MediatR;
using ShowSphere.Application.Common;

namespace ShowSphere.Application.Features.Users.Commands;

public record UpdateProfileCommand(Guid UserId, string FirstName, string LastName, string? Phone)
    : IRequest<Result<bool>>;

public record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword)
    : IRequest<Result<bool>>;

public record ForgotPasswordCommand(string Email)
    : IRequest<Result<bool>>;

public record ResetPasswordCommand(string Token, string NewPassword)
    : IRequest<Result<bool>>;
