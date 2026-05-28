using MediatR;
using Microsoft.EntityFrameworkCore;
using ShowSphere.Application.Common;
using ShowSphere.Application.Features.Movies.DTOs;
using ShowSphere.Application.Features.Movies.Queries;

namespace ShowSphere.Application.Features.Movies.Handlers;

public class GetMoviesQueryHandler : IRequestHandler<GetMoviesQuery, Result<PagedResult<MovieListDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetMoviesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<MovieListDto>>> Handle(GetMoviesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = _context.Movies
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .Where(m => m.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Language))
            query = query.Where(m => m.Language == filter.Language);

        if (!string.IsNullOrWhiteSpace(filter.Genre))
            query = query.Where(m => m.MovieGenres.Any(mg => mg.Genre.Name == filter.Genre));

        // For search: fetch all matching language/genre first, then apply fuzzy matching in memory
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchLower = filter.Search.ToLower();
            // First try case-insensitive contains in DB
            var dbFiltered = query.Where(m => m.Title.ToLower().Contains(searchLower) || m.Description.ToLower().Contains(searchLower));
            var dbCount = await dbFiltered.CountAsync(cancellationToken);

            if (dbCount > 0)
            {
                query = dbFiltered;
            }
            else
            {
                // Fuzzy match: load candidates and filter in memory
                var allMovies = await query
                    .Select(m => new { m.Id, m.Title })
                    .ToListAsync(cancellationToken);

                var matchingIds = allMovies
                    .Where(m => FuzzyMatch(searchLower, m.Title.ToLower(), 0.7))
                    .Select(m => m.Id)
                    .ToList();

                if (matchingIds.Count > 0)
                {
                    query = _context.Movies
                        .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                        .Where(m => m.IsActive && matchingIds.Contains(m.Id));

                    if (!string.IsNullOrWhiteSpace(filter.Language))
                        query = query.Where(m => m.Language == filter.Language);
                    if (!string.IsNullOrWhiteSpace(filter.Genre))
                        query = query.Where(m => m.MovieGenres.Any(mg => mg.Genre.Name == filter.Genre));
                }
                else
                {
                    query = query.Where(m => false); // no results
                }
            }
        }

        query = filter.SortBy?.ToLower() switch
        {
            "title" => filter.SortDescending ? query.OrderByDescending(m => m.Title) : query.OrderBy(m => m.Title),
            "rating" => filter.SortDescending ? query.OrderByDescending(m => (double)m.AverageRating) : query.OrderBy(m => (double)m.AverageRating),
            "releasedate" => filter.SortDescending ? query.OrderByDescending(m => m.ReleaseDate) : query.OrderBy(m => m.ReleaseDate),
            _ => query.OrderByDescending(m => m.ReleaseDate)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var movies = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(m => new MovieListDto(
                m.Id, m.Title, m.PosterUrl, m.DurationMinutes,
                m.Language, m.Certificate.ToString(), m.ReleaseDate,
                m.AverageRating, m.MovieGenres.Select(mg => mg.Genre.Name).ToList()))
            .ToListAsync(cancellationToken);

        return Result<PagedResult<MovieListDto>>.Success(new PagedResult<MovieListDto>
        {
            Items = movies,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        });
    }

    /// <summary>
    /// Fuzzy match using Levenshtein distance. Returns true if similarity >= threshold.
    /// Checks both full title and individual words.
    /// </summary>
    private static bool FuzzyMatch(string search, string target, double threshold)
    {
        // Check if any word in target is similar to search
        var targetWords = target.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var word in targetWords)
        {
            if (Similarity(search, word) >= threshold)
                return true;
        }
        // Also check full title similarity
        if (Similarity(search, target) >= threshold)
            return true;
        // Check if search matches start of any word
        foreach (var word in targetWords)
        {
            if (word.StartsWith(search) || search.StartsWith(word))
                return true;
        }
        return false;
    }

    private static double Similarity(string s, string t)
    {
        if (string.IsNullOrEmpty(s) && string.IsNullOrEmpty(t)) return 1.0;
        if (string.IsNullOrEmpty(s) || string.IsNullOrEmpty(t)) return 0.0;

        int maxLen = Math.Max(s.Length, t.Length);
        if (maxLen == 0) return 1.0;

        int distance = LevenshteinDistance(s, t);
        return 1.0 - (double)distance / maxLen;
    }

    private static int LevenshteinDistance(string s, string t)
    {
        int n = s.Length, m = t.Length;
        var d = new int[n + 1, m + 1];

        for (int i = 0; i <= n; i++) d[i, 0] = i;
        for (int j = 0; j <= m; j++) d[0, j] = j;

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = s[i - 1] == t[j - 1] ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        return d[n, m];
    }
}

public class GetMovieByIdQueryHandler : IRequestHandler<GetMovieByIdQuery, Result<MovieDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMovieByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<MovieDto>> Handle(GetMovieByIdQuery request, CancellationToken cancellationToken)
    {
        var movie = await _context.Movies
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .Include(m => m.MovieCasts).ThenInclude(mc => mc.Cast)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (movie == null)
            return Result<MovieDto>.Failure("Movie not found", 404);

        var dto = new MovieDto(
            movie.Id, movie.Title, movie.Description, movie.PosterUrl, movie.TrailerUrl,
            movie.DurationMinutes, movie.Language, movie.Certificate.ToString(),
            movie.ReleaseDate, movie.AverageRating, movie.TotalReviews,
            movie.MovieGenres.Select(mg => mg.Genre.Name).ToList(),
            movie.MovieCasts.Select(mc => new CastDto(mc.Cast.Id, mc.Cast.Name, mc.Role, mc.Cast.PhotoUrl)).ToList());

        return Result<MovieDto>.Success(dto);
    }
}

public class GetNowShowingQueryHandler : IRequestHandler<GetNowShowingQuery, Result<PagedResult<MovieListDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetNowShowingQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<MovieListDto>>> Handle(GetNowShowingQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var query = _context.Movies
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .Where(m => m.IsActive && m.ReleaseDate <= now)
            .Where(m => m.Shows.Any(s => s.StartTime >= now && s.IsActive));

        if (!string.IsNullOrWhiteSpace(request.City))
            query = query.Where(m => m.Shows.Any(s => s.Screen.Theater.City == request.City));

        var totalCount = await query.CountAsync(cancellationToken);
        var movies = await query
            .OrderByDescending(m => (double)m.AverageRating)
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

public class GetUpcomingMoviesQueryHandler : IRequestHandler<GetUpcomingMoviesQuery, Result<PagedResult<MovieListDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetUpcomingMoviesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<MovieListDto>>> Handle(GetUpcomingMoviesQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var query = _context.Movies
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .Where(m => m.IsActive && m.ReleaseDate > now);

        var totalCount = await query.CountAsync(cancellationToken);
        var movies = await query
            .OrderBy(m => m.ReleaseDate)
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
