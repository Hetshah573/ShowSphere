using MediatR;
using ShowSphere.Application.Common;
using ShowSphere.Application.Features.Movies.DTOs;
using ShowSphere.Domain.Enums;

namespace ShowSphere.Application.Features.Movies.Commands;

public record CreateMovieCommand(
    string Title,
    string Description,
    string? PosterUrl,
    string? TrailerUrl,
    int DurationMinutes,
    string Language,
    MovieCertificate Certificate,
    DateTime ReleaseDate,
    List<int> GenreIds,
    List<MovieCastRequest> Cast) : IRequest<Result<MovieDto>>;

public record UpdateMovieCommand(
    Guid Id,
    string Title,
    string Description,
    string? PosterUrl,
    string? TrailerUrl,
    int DurationMinutes,
    string Language,
    MovieCertificate Certificate,
    DateTime ReleaseDate,
    bool IsActive,
    List<int> GenreIds) : IRequest<Result<MovieDto>>;

public record DeleteMovieCommand(Guid Id) : IRequest<Result<bool>>;
