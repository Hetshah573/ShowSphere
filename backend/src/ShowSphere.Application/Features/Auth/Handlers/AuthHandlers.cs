using MediatR;
using Microsoft.EntityFrameworkCore;
using ShowSphere.Application.Common;
using ShowSphere.Application.Common.Exceptions;
using ShowSphere.Application.Features.Auth.Commands;
using ShowSphere.Application.Features.Auth.DTOs;
using ShowSphere.Domain.Entities;
using ShowSphere.Domain.Interfaces;

namespace ShowSphere.Application.Features.Auth.Handlers;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IAuditService _auditService;

    public RegisterCommandHandler(IApplicationDbContext context, ITokenService tokenService, IAuditService auditService)
    {
        _context = context;
        _tokenService = tokenService;
        _auditService = auditService;
    }

    public async Task<Result<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), cancellationToken);

        if (existingUser != null)
            return Result<AuthResponse>.Failure("Email is already registered", 409);

        var user = new User
        {
            Email = request.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            RoleId = 2 // User role
        };

        _context.Users.Add(user);

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = _tokenService.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(user.Id, "Register", "User", user.Id.ToString());

        user.Role = new Role { Id = 2, Name = "User" };
        var accessToken = _tokenService.GenerateAccessToken(user);

        return Result<AuthResponse>.Success(new AuthResponse(
            user.Id, user.Email, user.FirstName, user.LastName, user.Phone,
            "User", accessToken, refreshToken.Token), 201);
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IAuditService _auditService;

    public LoginCommandHandler(IApplicationDbContext context, ITokenService tokenService, IAuditService auditService)
    {
        _context = context;
        _tokenService = tokenService;
        _auditService = auditService;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Result<AuthResponse>.Failure("Invalid email or password", 401);

        if (!user.IsActive)
            return Result<AuthResponse>.Failure("Account is deactivated", 403);

        // Revoke existing refresh tokens
        var existingTokens = await _context.RefreshTokens
            .Where(t => t.UserId == user.Id && !t.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in existingTokens)
            token.IsRevoked = true;

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = _tokenService.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(user.Id, "Login", "User", user.Id.ToString());

        var accessToken = _tokenService.GenerateAccessToken(user);

        return Result<AuthResponse>.Success(new AuthResponse(
            user.Id, user.Email, user.FirstName, user.LastName, user.Phone,
            user.Role.Name, accessToken, refreshToken.Token));
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(IApplicationDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existingToken = await _context.RefreshTokens
            .Include(t => t.User).ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken, cancellationToken);

        if (existingToken == null || existingToken.IsRevoked || existingToken.ExpiresAt < DateTime.UtcNow)
            return Result<AuthResponse>.Failure("Invalid or expired refresh token", 401);

        existingToken.IsRevoked = true;
        existingToken.ReplacedByToken = _tokenService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            UserId = existingToken.UserId,
            Token = existingToken.ReplacedByToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        var user = existingToken.User;
        var accessToken = _tokenService.GenerateAccessToken(user);

        return Result<AuthResponse>.Success(new AuthResponse(
            user.Id, user.Email, user.FirstName, user.LastName, user.Phone,
            user.Role.Name, accessToken, newRefreshToken.Token));
    }
}

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public LogoutCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken, cancellationToken);

        if (token != null)
        {
            token.IsRevoked = true;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Result<bool>.Success(true);
    }
}

public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, Result<AuthResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IAuditService _auditService;

    public GoogleLoginCommandHandler(
        IApplicationDbContext context,
        ITokenService tokenService,
        IGoogleAuthService googleAuthService,
        IAuditService auditService)
    {
        _context = context;
        _tokenService = tokenService;
        _googleAuthService = googleAuthService;
        _auditService = auditService;
    }

    public async Task<Result<AuthResponse>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var googleUser = await _googleAuthService.VerifyIdTokenAsync(request.IdToken);
        if (googleUser == null)
            return Result<AuthResponse>.Failure("Invalid Google token", 401);

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == googleUser.Email.ToLower(), cancellationToken);

        if (user == null)
        {
            // Create new user from Google profile
            user = new User
            {
                Email = googleUser.Email.ToLower(),
                PasswordHash = "", // No password for Google users
                FirstName = googleUser.FirstName,
                LastName = googleUser.LastName,
                RoleId = 2
            };
            _context.Users.Add(user);
            user.Role = new Role { Id = 2, Name = "User" };
            await _auditService.LogAsync(user.Id, "GoogleRegister", "User", user.Id.ToString());
        }
        else
        {
            if (!user.IsActive)
                return Result<AuthResponse>.Failure("Account is deactivated", 403);

            await _auditService.LogAsync(user.Id, "GoogleLogin", "User", user.Id.ToString());
        }

        // Revoke existing tokens
        var existingTokens = await _context.RefreshTokens
            .Where(t => t.UserId == user.Id && !t.IsRevoked)
            .ToListAsync(cancellationToken);
        foreach (var t in existingTokens)
            t.IsRevoked = true;

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = _tokenService.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(user);

        return Result<AuthResponse>.Success(new AuthResponse(
            user.Id, user.Email, user.FirstName, user.LastName, user.Phone,
            user.Role.Name, accessToken, refreshToken.Token));
    }
}
