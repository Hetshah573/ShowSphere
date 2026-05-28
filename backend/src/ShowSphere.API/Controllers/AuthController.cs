using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShowSphere.Application.Features.Auth.Commands;
using ShowSphere.Application.Features.Auth.DTOs;

namespace ShowSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterCommand(request.Email, request.Password, request.FirstName, request.LastName, request.Phone);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return StatusCode(result.StatusCode, result.Data);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        var command = new LogoutCommand(request.RefreshToken);
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("google")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var command = new GoogleLoginCommand(request.IdToken);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }
}
