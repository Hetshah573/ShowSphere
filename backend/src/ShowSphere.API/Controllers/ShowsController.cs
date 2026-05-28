using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShowSphere.Application.Common;
using ShowSphere.Application.Features.Shows.Commands;
using ShowSphere.Application.Features.Shows.DTOs;
using ShowSphere.Application.Features.Shows.Queries;

namespace ShowSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShowsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;

    public ShowsController(IMediator mediator, IApplicationDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    [HttpGet("movie/{movieId:guid}")]
    public async Task<IActionResult> GetShowsByMovie(Guid movieId, [FromQuery] string? city, [FromQuery] DateTime? date)
    {
        var utcDate = date.HasValue ? DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc) : (DateTime?)null;
        var result = await _mediator.Send(new GetShowsByMovieQuery(movieId, city, utcDate));
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(result.Data);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public async Task<IActionResult> GetAllShows([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _context.Shows
            .Include(s => s.Movie)
            .Include(s => s.Screen).ThenInclude(sc => sc.Theater)
            .Where(s => s.IsActive)
            .OrderByDescending(s => s.StartTime);

        var total = await query.CountAsync();
        var shows = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new ShowDto(s.Id, s.MovieId, s.Movie.Title, s.ScreenId, s.Screen.Name,
                s.Screen.Theater.Name, s.Screen.Theater.City, s.StartTime, s.EndTime,
                s.BasePrice, 0, s.Screen.TotalSeats))
            .ToListAsync();

        return Ok(new { items = shows, totalCount = total, page, pageSize, totalPages = (int)Math.Ceiling(total / (double)pageSize) });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateShow([FromBody] CreateShowRequest request)
    {
        var command = new CreateShowCommand(request.MovieId, request.ScreenId, request.StartTime, request.BasePrice);
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });
        return StatusCode(result.StatusCode, result.Data);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteShow(Guid id)
    {
        var result = await _mediator.Send(new DeleteShowCommand(id));
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });
        return NoContent();
    }
}
