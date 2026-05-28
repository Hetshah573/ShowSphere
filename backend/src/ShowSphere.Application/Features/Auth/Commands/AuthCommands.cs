using MediatR;
using ShowSphere.Application.Common;
using ShowSphere.Application.Features.Auth.DTOs;

namespace ShowSphere.Application.Features.Auth.Commands;

public record RegisterCommand(string Email, string Password, string FirstName, string LastName, string? Phone)
    : IRequest<Result<AuthResponse>>;

public record LoginCommand(string Email, string Password)
    : IRequest<Result<AuthResponse>>;

public record RefreshTokenCommand(string RefreshToken)
    : IRequest<Result<AuthResponse>>;

public record LogoutCommand(string RefreshToken)
    : IRequest<Result<bool>>;

public record GoogleLoginCommand(string IdToken)
    : IRequest<Result<AuthResponse>>;
