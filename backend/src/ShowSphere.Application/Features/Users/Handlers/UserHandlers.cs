using MediatR;
using Microsoft.EntityFrameworkCore;
using ShowSphere.Application.Common;
using ShowSphere.Application.Features.Users.Commands;
using ShowSphere.Application.Features.Users.DTOs;
using ShowSphere.Application.Features.Users.Queries;
using ShowSphere.Domain.Entities;
using ShowSphere.Domain.Interfaces;

namespace ShowSphere.Application.Features.Users.Handlers;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, Result<UserProfileDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUserProfileQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UserProfileDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            return Result<UserProfileDto>.Failure("User not found", 404);

        var totalBookings = await _context.Bookings.CountAsync(b => b.UserId == user.Id, cancellationToken);
        var totalReviews = await _context.Reviews.CountAsync(r => r.UserId == user.Id, cancellationToken);

        return Result<UserProfileDto>.Success(new UserProfileDto(
            user.Id, user.Email, user.FirstName, user.LastName, user.Phone,
            user.Role.Name, user.CreatedAt, totalBookings, totalReviews));
    }
}

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public UpdateProfileCommandHandler(IApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<Result<bool>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user == null)
            return Result<bool>.Failure("User not found", 404);

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Phone = request.Phone;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(user.Id, "UpdateProfile", "User", user.Id.ToString());

        return Result<bool>.Success(true);
    }
}

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public ChangePasswordCommandHandler(IApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<Result<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user == null)
            return Result<bool>.Failure("User not found", 404);

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return Result<bool>.Failure("Current password is incorrect", 400);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(user.Id, "ChangePassword", "User", user.Id.ToString());

        return Result<bool>.Success(true);
    }
}

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(IApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<Result<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), cancellationToken);

        // Always return success to prevent email enumeration
        if (user == null)
            return Result<bool>.Success(true);

        var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _context.PasswordResetTokens.Add(resetToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _emailService.SendPasswordResetAsync(user.Email, $"{user.FirstName} {user.LastName}", token);

        return Result<bool>.Success(true);
    }
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public ResetPasswordCommandHandler(IApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<Result<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var resetToken = await _context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == request.Token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow, cancellationToken);

        if (resetToken == null)
            return Result<bool>.Failure("Invalid or expired reset token", 400);

        resetToken.IsUsed = true;
        resetToken.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        resetToken.User.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(resetToken.UserId, "ResetPassword", "User", resetToken.UserId.ToString());

        return Result<bool>.Success(true);
    }
}
