using MediatR;
using ShowSphere.Application.Common;
using ShowSphere.Application.Features.Users.DTOs;

namespace ShowSphere.Application.Features.Users.Queries;

public record GetUserProfileQuery(Guid UserId) : IRequest<Result<UserProfileDto>>;
