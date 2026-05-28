using MediatR;
using ShowSphere.Application.Common;
using ShowSphere.Application.Features.Shows.DTOs;

namespace ShowSphere.Application.Features.Shows.Commands;

public record CreateShowCommand(Guid MovieId, Guid ScreenId, DateTime StartTime, decimal BasePrice)
    : IRequest<Result<ShowDto>>;

public record DeleteShowCommand(Guid ShowId) : IRequest<Result<bool>>;
