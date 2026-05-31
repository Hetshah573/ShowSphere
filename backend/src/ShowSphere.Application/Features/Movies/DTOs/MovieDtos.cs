using ShowSphere.Domain.Enums;

namespace ShowSphere.Application.Features.Movies.DTOs;

public record MovieDto(
    Guid Id,
    string Title,
    string Description,
    string? PosterUrl,
    string? TrailerUrl,
    int DurationMinutes,
    string Language,
    string Certificate,
    DateTime ReleaseDate,
    decimal AverageRating,
    int TotalReviews,
    List<string> Genres,
    List<CastDto> Cast);

public record MovieListDto(
    Guid Id,
    string Title,
    string? PosterUrl,
    int DurationMinutes,
    string Language,
    string Certificate,
    DateTime ReleaseDate,
    decimal AverageRating,
    List<string> Genres);

public record CastDto(
    Guid Id,
    string Name,
    string Role,
    string? PhotoUrl);

public record CreateMovieRequest(
    string Title,
    string Description,
    string? PosterUrl,
    string? TrailerUrl,
    int DurationMinutes,
    string Language,
    MovieCertificate Certificate,
    DateTime ReleaseDate,
    List<int> GenreIds,
    List<MovieCastRequest> Cast);

public record MovieCastRequest(
    Guid CastId,
    string Role);

public record UpdateMovieRequest(
    string Title,
    string Description,
    string? PosterUrl,
    string? TrailerUrl,
    int DurationMinutes,
    string Language,
    MovieCertificate Certificate,
    DateTime ReleaseDate,
    bool IsActive,
    List<int> GenreIds);

public record MovieFilterRequest(
    string? Search,
    string? Language,
    string? Genre,
    string? City,
    string? SortBy,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? TimeSlot = null,
    string? TheaterId = null,
    bool? HasAvailableShows = null,
    bool SortDescending = true,
    int Page = 1,
    int PageSize = 12);
