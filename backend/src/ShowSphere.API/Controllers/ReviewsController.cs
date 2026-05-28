using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShowSphere.Application.Features.Reviews.DTOs;
using ShowSphere.Application.Features.Reviews.Handlers;

namespace ShowSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReviewsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("movie/{movieId:guid}")]
    public async Task<IActionResult> GetMovieReviews(Guid movieId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetMovieReviewsQuery(movieId, page, pageSize));
        return Ok(result.Data);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new CreateReviewCommand(userId, request.MovieId, request.Rating, request.Comment);
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });
        return StatusCode(result.StatusCode, result.Data);
    }
}
