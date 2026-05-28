using MediatR;
using Microsoft.EntityFrameworkCore;
using ShowSphere.Application.Common;
using ShowSphere.Application.Features.Shows.Commands;
using ShowSphere.Application.Features.Shows.DTOs;
using ShowSphere.Application.Features.Shows.Queries;
using ShowSphere.Domain.Entities;
using ShowSphere.Domain.Enums;

namespace ShowSphere.Application.Features.Shows.Handlers;

public class GetShowsByMovieQueryHandler : IRequestHandler<GetShowsByMovieQuery, Result<List<ShowsByMovieDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetShowsByMovieQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ShowsByMovieDto>>> Handle(GetShowsByMovieQuery request, CancellationToken cancellationToken)
    {
        var date = DateTime.SpecifyKind(
            request.Date.HasValue ? request.Date.Value.Date : DateTime.UtcNow.Date,
            DateTimeKind.Utc);
        var nextDate = date.AddDays(1);

        var query = _context.Shows
            .Include(s => s.Screen).ThenInclude(sc => sc.Theater)
            .Include(s => s.Screen).ThenInclude(sc => sc.Seats)
            .Where(s => s.MovieId == request.MovieId && s.IsActive && s.StartTime >= date && s.StartTime < nextDate);

        if (!string.IsNullOrWhiteSpace(request.City))
            query = query.Where(s => s.Screen.Theater.City == request.City);

        var shows = await query.ToListAsync(cancellationToken);

        // Get booked seat counts
        var showIds = shows.Select(s => s.Id).ToList();
        var bookedCounts = await _context.BookingSeats
            .Where(bs => showIds.Contains(bs.Booking.ShowId)
                && (bs.Status == BookingStatus.Confirmed
                    || (bs.Status == BookingStatus.Pending && bs.Booking.ExpiresAt > DateTime.UtcNow)))
            .GroupBy(bs => bs.Booking.ShowId)
            .Select(g => new { ShowId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var result = shows
            .GroupBy(s => s.Screen.Theater)
            .Select(g => new ShowsByMovieDto(
                g.Key.Name,
                g.Key.Address,
                g.Key.City,
                g.Select(s => new ShowTimingDto(
                    s.Id,
                    s.Screen.Name,
                    s.Screen.ScreenType.ToString(),
                    s.StartTime,
                    s.BasePrice,
                    s.Screen.TotalSeats - (bookedCounts.FirstOrDefault(bc => bc.ShowId == s.Id)?.Count ?? 0)
                )).OrderBy(st => st.StartTime).ToList()
            )).ToList();

        return Result<List<ShowsByMovieDto>>.Success(result);
    }
}

public class CreateShowCommandHandler : IRequestHandler<CreateShowCommand, Result<ShowDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateShowCommandHandler(IApplicationDbContext context) { _context = context; }

    public async Task<Result<ShowDto>> Handle(CreateShowCommand request, CancellationToken cancellationToken)
    {
        var movie = await _context.Movies.FindAsync(new object[] { request.MovieId }, cancellationToken);
        if (movie == null)
            return Result<ShowDto>.Failure("Movie not found", 404);

        var screen = await _context.Screens
            .Include(s => s.Theater)
            .FirstOrDefaultAsync(s => s.Id == request.ScreenId, cancellationToken);
        if (screen == null)
            return Result<ShowDto>.Failure("Screen not found", 404);

        var startTime = DateTime.SpecifyKind(request.StartTime, DateTimeKind.Utc);
        var endTime = startTime.AddMinutes(movie.DurationMinutes + 15); // 15 min buffer

        // Check for overlapping shows on the same screen
        var overlap = await _context.Shows.AnyAsync(s =>
            s.ScreenId == request.ScreenId && s.IsActive &&
            s.StartTime < endTime && s.EndTime > startTime, cancellationToken);

        if (overlap)
            return Result<ShowDto>.Failure("Time slot overlaps with an existing show on this screen", 409);

        var show = new Show
        {
            MovieId = request.MovieId,
            ScreenId = request.ScreenId,
            StartTime = startTime,
            EndTime = endTime,
            BasePrice = request.BasePrice
        };

        _context.Shows.Add(show);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new ShowDto(show.Id, movie.Id, movie.Title, screen.Id, screen.Name,
            screen.Theater.Name, screen.Theater.City, show.StartTime, show.EndTime,
            show.BasePrice, screen.TotalSeats, screen.TotalSeats);

        return Result<ShowDto>.Success(dto, 201);
    }
}

public class DeleteShowCommandHandler : IRequestHandler<DeleteShowCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public DeleteShowCommandHandler(IApplicationDbContext context) { _context = context; }

    public async Task<Result<bool>> Handle(DeleteShowCommand request, CancellationToken cancellationToken)
    {
        var show = await _context.Shows.FindAsync(new object[] { request.ShowId }, cancellationToken);
        if (show == null)
            return Result<bool>.Failure("Show not found", 404);

        // Don't delete shows with confirmed bookings
        var hasBookings = await _context.Bookings
            .AnyAsync(b => b.ShowId == show.Id && b.Status == BookingStatus.Confirmed, cancellationToken);
        if (hasBookings)
            return Result<bool>.Failure("Cannot delete show with confirmed bookings", 400);

        show.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
