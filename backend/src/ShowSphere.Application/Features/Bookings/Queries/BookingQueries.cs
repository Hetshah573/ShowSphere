using MediatR;
using ShowSphere.Application.Common;
using ShowSphere.Application.Features.Bookings.DTOs;

namespace ShowSphere.Application.Features.Bookings.Queries;

public record GetSeatAvailabilityQuery(Guid ShowId) : IRequest<Result<List<SeatAvailabilityDto>>>;

public record GetBookingByIdQuery(Guid BookingId, Guid UserId) : IRequest<Result<BookingDto>>;

public record GetBookingHistoryQuery(Guid UserId, int Page = 1, int PageSize = 10) : IRequest<Result<PagedResult<BookingHistoryDto>>>;
