using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShowSphere.Application.Common;
using ShowSphere.Domain.Entities;
using System.Security.Claims;

namespace ShowSphere.API.Controllers;

[ApiController]
[Route("api/movies")]
[Authorize]
public class MovieNotificationsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public MovieNotificationsController(IApplicationDbContext context)
    {
        _context = context;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Subscribe to get notified when a movie releases.
    /// </summary>
    [HttpPost("{movieId:guid}/notify")]
    public async Task<IActionResult> Subscribe(Guid movieId)
    {
        var userId = GetUserId();

        var exists = await _context.MovieNotificationSubscriptions
            .AnyAsync(s => s.UserId == userId && s.MovieId == movieId);

        if (exists)
            return Ok(new { subscribed = true });

        _context.MovieNotificationSubscriptions.Add(new MovieNotificationSubscription
        {
            UserId = userId,
            MovieId = movieId
        });
        await _context.SaveChangesAsync();

        return Ok(new { subscribed = true });
    }

    /// <summary>
    /// Check if user is subscribed to a movie notification.
    /// </summary>
    [HttpGet("{movieId:guid}/notify")]
    public async Task<IActionResult> GetStatus(Guid movieId)
    {
        var userId = GetUserId();
        var exists = await _context.MovieNotificationSubscriptions
            .AnyAsync(s => s.UserId == userId && s.MovieId == movieId);

        return Ok(new { subscribed = exists });
    }

    /// <summary>
    /// Unsubscribe from movie release notification.
    /// </summary>
    [HttpDelete("{movieId:guid}/notify")]
    public async Task<IActionResult> Unsubscribe(Guid movieId)
    {
        var userId = GetUserId();
        var sub = await _context.MovieNotificationSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.MovieId == movieId);

        if (sub != null)
        {
            _context.MovieNotificationSubscriptions.Remove(sub);
            await _context.SaveChangesAsync();
        }

        return Ok(new { subscribed = false });
    }
}
