using MediatR;
using ShowSphere.Application.Common;
using ShowSphere.Application.Features.Bookings.DTOs;
using ShowSphere.Domain.Enums;

namespace ShowSphere.Application.Features.Bookings.Commands;

public record CreateBookingCommand(
    Guid UserId,
    Guid ShowId,
    List<Guid> SeatIds,
    PaymentMethod PaymentMethod) : IRequest<Result<BookingDto>>;

public record CancelBookingCommand(Guid BookingId, Guid UserId) : IRequest<Result<bool>>;

public record ConfirmPaymentCommand(Guid BookingId, Guid UserId, string TransactionId) : IRequest<Result<BookingDto>>;
