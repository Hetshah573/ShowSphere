using MediatR;
using ShowSphere.Application.Common;
using ShowSphere.Application.Features.Movies.DTOs;

namespace ShowSphere.Application.Features.Movies.Queries;

public record GetMoviesQuery(MovieFilterRequest Filter) : IRequest<Result<PagedResult<MovieListDto>>>;

public record GetMovieByIdQuery(Guid Id) : IRequest<Result<MovieDto>>;

public record GetNowShowingQuery(string? City, int Page = 1, int PageSize = 12) : IRequest<Result<PagedResult<MovieListDto>>>;

public record GetUpcomingMoviesQuery(int Page = 1, int PageSize = 12) : IRequest<Result<PagedResult<MovieListDto>>>;
