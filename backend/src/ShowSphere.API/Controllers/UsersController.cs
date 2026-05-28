using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShowSphere.Application.Features.Users.Commands;
using ShowSphere.Application.Features.Users.DTOs;
using ShowSphere.Application.Features.Users.Queries;
using ShowSphere.Domain.Interfaces;

namespace ShowSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public UsersController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _mediator.Send(new GetUserProfileQuery(_currentUser.UserId!.Value));
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(result.Data);
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var command = new UpdateProfileCommand(_currentUser.UserId!.Value, request.FirstName, request.LastName, request.Phone);
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(new { message = "Profile updated successfully" });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var command = new ChangePasswordCommand(_currentUser.UserId!.Value, request.CurrentPassword, request.NewPassword);
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(new { message = "Password changed successfully" });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var command = new ForgotPasswordCommand(request.Email);
        var result = await _mediator.Send(command);
        return Ok(new { message = "If an account exists with that email, a reset link has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var command = new ResetPasswordCommand(request.Token, request.NewPassword);
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(new { message = "Password reset successfully" });
    }
}
