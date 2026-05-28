using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShowSphere.Application.Features.Wishlist.Commands;
using ShowSphere.Application.Features.Wishlist.Queries;
using ShowSphere.Domain.Interfaces;

namespace ShowSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public WishlistController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetWishlist([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetWishlistQuery(_currentUser.UserId!.Value, page, pageSize));
        return Ok(result.Data);
    }

    [HttpGet("{movieId:guid}")]
    public async Task<IActionResult> IsInWishlist(Guid movieId)
    {
        var result = await _mediator.Send(new IsInWishlistQuery(_currentUser.UserId!.Value, movieId));
        return Ok(new { isInWishlist = result.Data });
    }

    [HttpPost("{movieId:guid}")]
    public async Task<IActionResult> AddToWishlist(Guid movieId)
    {
        var result = await _mediator.Send(new AddToWishlistCommand(_currentUser.UserId!.Value, movieId));
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(new { message = "Added to wishlist" });
    }

    [HttpDelete("{movieId:guid}")]
    public async Task<IActionResult> RemoveFromWishlist(Guid movieId)
    {
        var result = await _mediator.Send(new RemoveFromWishlistCommand(_currentUser.UserId!.Value, movieId));
        return Ok(new { message = "Removed from wishlist" });
    }
}
