using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShowSphere.Application.Common;
using ShowSphere.Application.Features.Movies.Commands;
using ShowSphere.Application.Features.Movies.DTOs;
using ShowSphere.Application.Features.Movies.Queries;

namespace ShowSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;

    public MoviesController(IMediator mediator, IApplicationDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    [HttpGet("genres")]
    public async Task<IActionResult> GetGenres()
    {
        var genres = await _context.Genres.Select(g => new { g.Id, g.Name }).ToListAsync();
        return Ok(genres);
    }

    [HttpGet]
    public async Task<IActionResult> GetMovies([FromQuery] MovieFilterRequest filter)
    {
        var result = await _mediator.Send(new GetMoviesQuery(filter));
        return Ok(result.Data);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMovie(Guid id)
    {
        var result = await _mediator.Send(new GetMovieByIdQuery(id));
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(result.Data);
    }

    [HttpGet("now-showing")]
    public async Task<IActionResult> GetNowShowing([FromQuery] string? city, [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        var result = await _mediator.Send(new GetNowShowingQuery(city, page, pageSize));
        return Ok(result.Data);
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming([FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        var result = await _mediator.Send(new GetUpcomingMoviesQuery(page, pageSize));
        return Ok(result.Data);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateMovie([FromBody] CreateMovieRequest request)
    {
        var command = new CreateMovieCommand(
            request.Title, request.Description, request.PosterUrl, request.TrailerUrl,
            request.DurationMinutes, request.Language, request.Certificate,
            request.ReleaseDate, request.GenreIds, request.Cast);

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });
        return StatusCode(result.StatusCode, result.Data);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateMovie(Guid id, [FromBody] UpdateMovieRequest request)
    {
        var command = new UpdateMovieCommand(
            id, request.Title, request.Description, request.PosterUrl, request.TrailerUrl,
            request.DurationMinutes, request.Language, request.Certificate,
            request.ReleaseDate, request.IsActive, request.GenreIds);

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(result.Data);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteMovie(Guid id)
    {
        var result = await _mediator.Send(new DeleteMovieCommand(id));
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });
        return NoContent();
    }
}
