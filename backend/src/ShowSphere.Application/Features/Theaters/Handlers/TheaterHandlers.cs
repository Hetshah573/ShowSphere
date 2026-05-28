using MediatR;
using Microsoft.EntityFrameworkCore;
using ShowSphere.Application.Common;
using ShowSphere.Application.Features.Theaters.DTOs;

namespace ShowSphere.Application.Features.Theaters.Handlers;

public record GetTheatersQuery(string? City) : IRequest<Result<List<TheaterListDto>>>;

public record GetTheaterByIdQuery(Guid Id) : IRequest<Result<TheaterDto>>;

public class GetTheatersQueryHandler : IRequestHandler<GetTheatersQuery, Result<List<TheaterListDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetTheatersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<TheaterListDto>>> Handle(GetTheatersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Theaters
            .Include(t => t.Screens)
            .Where(t => t.IsActive);

        if (!string.IsNullOrWhiteSpace(request.City))
            query = query.Where(t => t.City == request.City);

        var theaters = await query
            .Select(t => new TheaterListDto(t.Id, t.Name, t.Address, t.City, t.Screens.Count))
            .ToListAsync(cancellationToken);

        return Result<List<TheaterListDto>>.Success(theaters);
    }
}

public class GetTheaterByIdQueryHandler : IRequestHandler<GetTheaterByIdQuery, Result<TheaterDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTheaterByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TheaterDto>> Handle(GetTheaterByIdQuery request, CancellationToken cancellationToken)
    {
        var theater = await _context.Theaters
            .Include(t => t.Screens)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (theater == null)
            return Result<TheaterDto>.Failure("Theater not found", 404);

        var dto = new TheaterDto(
            theater.Id, theater.Name, theater.Address, theater.City, theater.State, theater.PinCode,
            theater.Screens.Select(s => new ScreenDto(s.Id, s.Name, s.TotalSeats, s.ScreenType.ToString())).ToList());

        return Result<TheaterDto>.Success(dto);
    }
}
