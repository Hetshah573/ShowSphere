using MediatR;
using Microsoft.EntityFrameworkCore;
using ShowSphere.Application.Common;
using ShowSphere.Application.Common.Exceptions;
using ShowSphere.Application.Features.Movies.Commands;
using ShowSphere.Application.Features.Movies.DTOs;
using ShowSphere.Domain.Entities;
using ShowSphere.Domain.Interfaces;

namespace ShowSphere.Application.Features.Movies.Handlers;

public class CreateMovieCommandHandler : IRequestHandler<CreateMovieCommand, Result<MovieDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public CreateMovieCommandHandler(IApplicationDbContext context, IAuditService auditService, ICurrentUserService currentUser)
    {
        _context = context;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<Result<MovieDto>> Handle(CreateMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = new Movie
        {
            Title = request.Title,
            Description = request.Description,
            PosterUrl = request.PosterUrl,
            TrailerUrl = request.TrailerUrl,
            DurationMinutes = request.DurationMinutes,
            Language = request.Language,
            Certificate = request.Certificate,
            ReleaseDate = DateTime.SpecifyKind(request.ReleaseDate, DateTimeKind.Utc)
        };

        foreach (var genreId in request.GenreIds)
        {
            movie.MovieGenres.Add(new MovieGenre { MovieId = movie.Id, GenreId = genreId });
        }

        foreach (var cast in request.Cast)
        {
            movie.MovieCasts.Add(new MovieCast { MovieId = movie.Id, CastId = cast.CastId, Role = cast.Role });
        }

        _context.Movies.Add(movie);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(_currentUser.UserId, "CreateMovie", "Movie", movie.Id.ToString());

        var createdMovie = await _context.Movies
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .Include(m => m.MovieCasts).ThenInclude(mc => mc.Cast)
            .FirstAsync(m => m.Id == movie.Id, cancellationToken);

        return Result<MovieDto>.Success(new MovieDto(
            createdMovie.Id, createdMovie.Title, createdMovie.Description,
            createdMovie.PosterUrl, createdMovie.TrailerUrl,
            createdMovie.DurationMinutes, createdMovie.Language,
            createdMovie.Certificate.ToString(), createdMovie.ReleaseDate,
            createdMovie.AverageRating, createdMovie.TotalReviews,
            createdMovie.MovieGenres.Select(mg => mg.Genre.Name).ToList(),
            createdMovie.MovieCasts.Select(mc => new CastDto(mc.Cast.Id, mc.Cast.Name, mc.Role, mc.Cast.PhotoUrl)).ToList()), 201);
    }
}

public class DeleteMovieCommandHandler : IRequestHandler<DeleteMovieCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public DeleteMovieCommandHandler(IApplicationDbContext context, IAuditService auditService, ICurrentUserService currentUser)
    {
        _context = context;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(DeleteMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = await _context.Movies.FindAsync(new object[] { request.Id }, cancellationToken);
        if (movie == null)
            return Result<bool>.Failure("Movie not found", 404);

        movie.IsActive = false;
        movie.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(_currentUser.UserId, "DeleteMovie", "Movie", movie.Id.ToString());

        return Result<bool>.Success(true);
    }
}

public class UpdateMovieCommandHandler : IRequestHandler<UpdateMovieCommand, Result<MovieDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public UpdateMovieCommandHandler(IApplicationDbContext context, IAuditService auditService, ICurrentUserService currentUser)
    {
        _context = context;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<Result<MovieDto>> Handle(UpdateMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = await _context.Movies
            .Include(m => m.MovieGenres)
            .Include(m => m.MovieCasts).ThenInclude(mc => mc.Cast)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (movie == null)
            return Result<MovieDto>.Failure("Movie not found", 404);

        movie.Title = request.Title;
        movie.Description = request.Description;
        movie.PosterUrl = request.PosterUrl;
        movie.TrailerUrl = request.TrailerUrl;
        movie.DurationMinutes = request.DurationMinutes;
        movie.Language = request.Language;
        movie.Certificate = request.Certificate;
        movie.ReleaseDate = DateTime.SpecifyKind(request.ReleaseDate, DateTimeKind.Utc);
        movie.IsActive = request.IsActive;
        movie.UpdatedAt = DateTime.UtcNow;

        // Update genres
        movie.MovieGenres.Clear();
        foreach (var genreId in request.GenreIds)
        {
            movie.MovieGenres.Add(new MovieGenre { MovieId = movie.Id, GenreId = genreId });
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(_currentUser.UserId, "UpdateMovie", "Movie", movie.Id.ToString());

        var updated = await _context.Movies
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .Include(m => m.MovieCasts).ThenInclude(mc => mc.Cast)
            .FirstAsync(m => m.Id == movie.Id, cancellationToken);

        return Result<MovieDto>.Success(new MovieDto(
            updated.Id, updated.Title, updated.Description,
            updated.PosterUrl, updated.TrailerUrl,
            updated.DurationMinutes, updated.Language,
            updated.Certificate.ToString(), updated.ReleaseDate,
            updated.AverageRating, updated.TotalReviews,
            updated.MovieGenres.Select(mg => mg.Genre.Name).ToList(),
            updated.MovieCasts.Select(mc => new CastDto(mc.Cast.Id, mc.Cast.Name, mc.Role, mc.Cast.PhotoUrl)).ToList()));
    }
}
