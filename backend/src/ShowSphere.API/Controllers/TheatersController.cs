using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShowSphere.Application.Features.Theaters.Handlers;

namespace ShowSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TheatersController : ControllerBase
{
    private readonly IMediator _mediator;

    public TheatersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetTheaters([FromQuery] string? city)
    {
        var result = await _mediator.Send(new GetTheatersQuery(city));
        return Ok(result.Data);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTheater(Guid id)
    {
        var result = await _mediator.Send(new GetTheaterByIdQuery(id));
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(result.Data);
    }
}
