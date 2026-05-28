using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShowSphere.Application.Common;
using ShowSphere.Domain.Enums;

namespace ShowSphere.API.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public DashboardController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var totalUsers = await _context.Users.CountAsync();
        var totalMovies = await _context.Movies.CountAsync(m => m.IsActive);
        var totalBookings = await _context.Bookings.CountAsync();
        var totalRevenue = (await _context.Payments
            .Where(p => p.Status == PaymentStatus.Completed)
            .Select(p => (double)p.Amount)
            .ToListAsync()).Sum();

        var todayBookings = await _context.Bookings
            .CountAsync(b => b.CreatedAt.Date == DateTime.UtcNow.Date);

        var todayRevenue = (await _context.Payments
            .Where(p => p.Status == PaymentStatus.Completed && p.PaidAt != null && p.PaidAt.Value.Date == DateTime.UtcNow.Date)
            .Select(p => (double)p.Amount)
            .ToListAsync()).Sum();

        // Booking status breakdown
        var confirmedBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Confirmed);
        var pendingBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Pending);
        var cancelledBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Cancelled);
        var expiredBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Expired);

        // Top movies by bookings
        var topMoviesRaw = await _context.Bookings
            .Where(b => b.Status == BookingStatus.Confirmed)
            .Include(b => b.Show).ThenInclude(s => s.Movie)
            .Select(b => new { b.Show.Movie.Id, b.Show.Movie.Title, b.Show.Movie.PosterUrl, b.TotalAmount })
            .ToListAsync();

        var topMovies = topMoviesRaw
            .GroupBy(b => new { b.Id, b.Title, b.PosterUrl })
            .Select(g => new
            {
                g.Key.Id,
                g.Key.Title,
                g.Key.PosterUrl,
                BookingCount = g.Count(),
                Revenue = g.Sum(b => b.TotalAmount)
            })
            .OrderByDescending(x => x.BookingCount)
            .Take(5)
            .ToList();

        // Revenue last 7 days
        var last7Days = Enumerable.Range(0, 7).Select(i => DateTime.UtcNow.Date.AddDays(-i)).ToList();
        var revenueRaw = await _context.Payments
            .Where(p => p.Status == PaymentStatus.Completed && p.PaidAt != null && p.PaidAt.Value.Date >= last7Days.Last())
            .Select(p => new { Date = p.PaidAt!.Value.Date, p.Amount })
            .ToListAsync();

        var revenueByDay = revenueRaw
            .GroupBy(p => p.Date)
            .Select(g => new { Date = g.Key, Revenue = g.Sum(p => p.Amount) })
            .ToList();

        var dailyRevenue = last7Days.OrderBy(d => d).Select(d => new
        {
            Date = d.ToString("MMM dd"),
            Revenue = revenueByDay.FirstOrDefault(r => r.Date == d)?.Revenue ?? 0m
        }).ToList();

        // Recent bookings
        var recentBookings = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Show).ThenInclude(s => s.Movie)
            .OrderByDescending(b => b.CreatedAt)
            .Take(10)
            .Select(b => new
            {
                b.Id,
                b.BookingNumber,
                UserName = $"{b.User.FirstName} {b.User.LastName}",
                MovieTitle = b.Show.Movie.Title,
                b.TotalAmount,
                Status = b.Status.ToString(),
                b.CreatedAt
            })
            .ToListAsync();

        // Upcoming shows count
        var upcomingShows = await _context.Shows.CountAsync(s => s.StartTime > DateTime.UtcNow);

        return Ok(new
        {
            totalUsers,
            totalMovies,
            totalBookings,
            totalRevenue,
            todayBookings,
            todayRevenue,
            confirmedBookings,
            pendingBookings,
            cancelledBookings,
            expiredBookings,
            upcomingShows,
            topMovies,
            dailyRevenue,
            recentBookings
        });
    }
}
