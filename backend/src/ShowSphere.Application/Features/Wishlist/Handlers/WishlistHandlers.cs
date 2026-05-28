using MediatR;
using Microsoft.EntityFrameworkCore;
using ShowSphere.Application.Common;
using ShowSphere.Application.Features.Movies.DTOs;
using ShowSphere.Application.Features.Wishlist.Commands;
using ShowSphere.Application.Features.Wishlist.Queries;

namespace ShowSphere.Application.Features.Wishlist.Handlers;

public class AddToWishlistCommandHandler : IRequestHandler<AddToWishlistCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public AddToWishlistCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(AddToWishlistCommand request, CancellationToken cancellationToken)
    {
        var movieExists = await _context.Movies.AnyAsync(m => m.Id == request.MovieId, cancellationToken);
        if (!movieExists)
            return Result<bool>.Failure("Movie not found", 404);

        var exists = await _context.Wishlists
            .AnyAsync(w => w.UserId == request.UserId && w.MovieId == request.MovieId, cancellationToken);

        if (exists)
            return Result<bool>.Success(true);

        _context.Wishlists.Add(new Domain.Entities.Wishlist
        {
            UserId = request.UserId,
            MovieId = request.MovieId
        });

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}

public class RemoveFromWishlistCommandHandler : IRequestHandler<RemoveFromWishlistCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public RemoveFromWishlistCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(RemoveFromWishlistCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.Wishlists
            .FirstOrDefaultAsync(w => w.UserId == request.UserId && w.MovieId == request.MovieId, cancellationToken);

        if (item != null)
        {
            _context.Wishlists.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Result<bool>.Success(true);
    }
}

public class GetWishlistQueryHandler : IRequestHandler<GetWishlistQuery, Result<PagedResult<MovieListDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetWishlistQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<MovieListDto>>> Handle(GetWishlistQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Wishlists
            .Where(w => w.UserId == request.UserId)
            .Include(w => w.Movie).ThenInclude(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .Select(w => w.Movie)
            .Where(m => m.IsActive);

        var totalCount = await query.CountAsync(cancellationToken);
        var movies = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new MovieListDto(
                m.Id, m.Title, m.PosterUrl, m.DurationMinutes,
                m.Language, m.Certificate.ToString(), m.ReleaseDate,
                m.AverageRating, m.MovieGenres.Select(mg => mg.Genre.Name).ToList()))
            .ToListAsync(cancellationToken);

        return Result<PagedResult<MovieListDto>>.Success(new PagedResult<MovieListDto>
        {
            Items = movies,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}

public class IsInWishlistQueryHandler : IRequestHandler<IsInWishlistQuery, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public IsInWishlistQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(IsInWishlistQuery request, CancellationToken cancellationToken)
    {
        var exists = await _context.Wishlists
            .AnyAsync(w => w.UserId == request.UserId && w.MovieId == request.MovieId, cancellationToken);

        return Result<bool>.Success(exists);
    }
}
