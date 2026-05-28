using MediatR;
using ShowSphere.Application.Common;
using ShowSphere.Application.Features.Movies.DTOs;

namespace ShowSphere.Application.Features.Wishlist.Queries;

public record GetWishlistQuery(Guid UserId, int Page = 1, int PageSize = 20) : IRequest<Result<PagedResult<MovieListDto>>>;

public record IsInWishlistQuery(Guid UserId, Guid MovieId) : IRequest<Result<bool>>;
