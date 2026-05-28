using MediatR;
using ShowSphere.Application.Common;
using ShowSphere.Application.Features.Movies.DTOs;

namespace ShowSphere.Application.Features.Wishlist.Commands;

public record AddToWishlistCommand(Guid UserId, Guid MovieId) : IRequest<Result<bool>>;

public record RemoveFromWishlistCommand(Guid UserId, Guid MovieId) : IRequest<Result<bool>>;
