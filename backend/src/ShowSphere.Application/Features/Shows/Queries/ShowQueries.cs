using MediatR;
using ShowSphere.Application.Common;
using ShowSphere.Application.Features.Shows.DTOs;

namespace ShowSphere.Application.Features.Shows.Queries;

public record GetShowsByMovieQuery(Guid MovieId, string? City, DateTime? Date) : IRequest<Result<List<ShowsByMovieDto>>>;
