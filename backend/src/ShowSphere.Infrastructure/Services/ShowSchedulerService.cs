using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShowSphere.Application.Common;
using ShowSphere.Domain.Entities;
using ShowSphere.Domain.Enums;
using ShowSphere.Domain.Interfaces;

namespace ShowSphere.Infrastructure.Services;

public class ShowSchedulerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ShowSchedulerService> _logger;
    private readonly TimeSpan _scheduledTime = new(20, 30, 0); // 2:00 AM IST (UTC+5:30)

    private static readonly double[] TimeSlots = { 9.0, 12.5, 15.5, 18.5, 21.5 };

    private static readonly Dictionary<ScreenType, decimal> BasePrices = new()
    {
        { ScreenType.Standard, 280m },
        { ScreenType.IMAX, 550m },
        { ScreenType.Dolby, 400m },
        { ScreenType.FourDX, 700m }
    };

    public ShowSchedulerService(IServiceScopeFactory scopeFactory, ILogger<ShowSchedulerService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[SCHEDULER] Show scheduler started. Will run daily at {Time}", _scheduledTime);

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = GetDelayUntilNextRun();
            _logger.LogInformation("[SCHEDULER] Next run in {Delay}", delay);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            await RunSchedulerAsync(stoppingToken);
        }
    }

    private TimeSpan GetDelayUntilNextRun()
    {
        var now = DateTime.UtcNow;
        var nextRun = now.Date.Add(_scheduledTime);

        if (nextRun <= now)
            nextRun = nextRun.AddDays(1);

        return nextRun - now;
    }

    private async Task RunSchedulerAsync(CancellationToken ct)
    {
        _logger.LogInformation("[SCHEDULER] Starting show scheduling run at {Time}", DateTime.UtcNow);

        var report = new SchedulerReport { RunTime = DateTime.UtcNow };

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            // 1. Delete past shows (keep ones with bookings for history, just deactivate those)
            var pastShows = await context.Shows
                .Include(s => s.Bookings)
                .Where(s => s.EndTime < DateTime.UtcNow)
                .ToListAsync(ct);

            var showsWithBookings = pastShows.Where(s => s.Bookings.Count > 0).ToList();
            var showsWithoutBookings = pastShows.Where(s => s.Bookings.Count == 0).ToList();

            // Deactivate shows that have bookings (needed for booking history)
            foreach (var show in showsWithBookings)
                show.IsActive = false;

            // Delete shows with no bookings entirely
            context.Shows.RemoveRange(showsWithoutBookings);

            report.DeactivatedShows = showsWithBookings.Count;
            report.DeletedShows = showsWithoutBookings.Count;

            // 2. Get eligible movies (released or releasing within 48 hours)
            var cutoffDate = DateTime.UtcNow.AddDays(2);
            var movies = await context.Movies
                .Where(m => m.IsActive && m.ReleaseDate <= cutoffDate)
                .ToListAsync(ct);

            report.EligibleMovies = movies.Count;

            // 3. Get all active screens with theaters
            var screens = await context.Screens
                .Include(s => s.Theater)
                .Where(s => s.Theater.IsActive)
                .ToListAsync(ct);

            // 4. Get existing shows for next 7 days to avoid duplicates
            var today = DateTime.UtcNow.Date;
            var endDate = today.AddDays(7);
            var existingShows = await context.Shows
                .Where(s => s.StartTime >= today && s.StartTime < endDate)
                .Select(s => new { s.ScreenId, s.MovieId, Date = s.StartTime.Date, s.StartTime })
                .ToListAsync(ct);

            var existingShowSet = new HashSet<string>(
                existingShows.Select(s => $"{s.ScreenId}|{s.MovieId}|{s.StartTime:yyyyMMddHHmm}"));

            // 5. Group screens by theater for fair distribution
            var screensByTheater = screens.GroupBy(s => s.TheaterId).ToList();
            var random = new Random();
            var showsCreated = new List<SchedulerShowInfo>();

            foreach (var movie in movies)
            {
                var movieStartDate = movie.ReleaseDate.Date > today ? movie.ReleaseDate.Date : today;

                for (var day = movieStartDate; day < endDate; day = day.AddDays(1))
                {
                    // Pick 1-2 random theaters for this movie on this day
                    var shuffledTheaters = screensByTheater.OrderBy(_ => random.Next()).ToList();
                    var theatersToAssign = Math.Min(2, shuffledTheaters.Count);

                    for (var t = 0; t < theatersToAssign; t++)
                    {
                        var theaterScreens = shuffledTheaters[t].ToList();
                        var screen = theaterScreens[random.Next(theaterScreens.Count)];

                        foreach (var hour in TimeSlots)
                        {
                            var startTime = day.AddHours(hour);
                            if (startTime < DateTime.UtcNow) continue;

                            var key = $"{screen.Id}|{movie.Id}|{startTime:yyyyMMddHHmm}";
                            if (existingShowSet.Contains(key)) continue;

                            // Check for screen time conflicts (within movie duration + buffer)
                            var endTime = startTime.AddMinutes(movie.DurationMinutes + 20);
                            var hasConflict = existingShows.Any(es =>
                                es.ScreenId == screen.Id &&
                                es.Date == day &&
                                Math.Abs((es.StartTime - startTime).TotalMinutes) < movie.DurationMinutes + 20);

                            if (hasConflict) continue;

                            var basePrice = BasePrices.GetValueOrDefault(screen.ScreenType, 280m);

                            var show = new Show
                            {
                                MovieId = movie.Id,
                                ScreenId = screen.Id,
                                StartTime = startTime,
                                EndTime = endTime,
                                BasePrice = basePrice,
                                IsActive = true
                            };

                            context.Shows.Add(show);
                            existingShowSet.Add(key);
                            existingShows.Add(new { ScreenId = screen.Id, MovieId = movie.Id, Date = day, StartTime = startTime });

                            showsCreated.Add(new SchedulerShowInfo
                            {
                                MovieTitle = movie.Title,
                                TheaterName = screen.Theater.Name,
                                ScreenName = screen.Name,
                                ScreenType = screen.ScreenType.ToString(),
                                StartTime = startTime,
                                BasePrice = basePrice
                            });
                        }
                    }
                }
            }

            report.ShowsCreated = showsCreated.Count;
            report.ShowDetails = showsCreated;
            report.IsSuccess = true;

            await context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "[SCHEDULER] Completed. Deactivated: {Deactivated}, Deleted: {Deleted}, Created: {Created}, Movies: {Movies}",
                report.DeactivatedShows, report.DeletedShows, report.ShowsCreated, report.EligibleMovies);

            // 6. Send report email to admin(s)
            await SendReportEmailAsync(context, emailService, report, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SCHEDULER] Failed during show scheduling");
            report.IsSuccess = false;
            report.ErrorMessage = ex.Message;

            // Try to send failure email
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                await SendReportEmailAsync(context, emailService, report, ct);
            }
            catch (Exception emailEx)
            {
                _logger.LogError(emailEx, "[SCHEDULER] Failed to send failure report email");
            }
        }
    }

    private static async Task SendReportEmailAsync(
        IApplicationDbContext context, IEmailService emailService, SchedulerReport report, CancellationToken ct)
    {
        var admins = await context.Users
            .Where(u => u.RoleId == 1 && u.IsActive)
            .Select(u => new { u.Email, u.FirstName })
            .ToListAsync(ct);

        foreach (var admin in admins)
        {
            await emailService.SendSchedulerReportAsync(admin.Email, admin.FirstName, report);
        }
    }

}
