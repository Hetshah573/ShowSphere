using ShowSphere.Domain.Enums;

namespace ShowSphere.Application.Features.Bookings.DTOs;

public record CreateBookingRequest(
    Guid ShowId,
    List<Guid> SeatIds,
    PaymentMethod PaymentMethod);

public record BookingDto(
    Guid Id,
    string BookingNumber,
    string MovieTitle,
    string TheaterName,
    string ScreenName,
    DateTime ShowTime,
    int TotalSeats,
    decimal TotalAmount,
    string Status,
    List<BookingSeatDto> Seats,
    string? QRCode,
    DateTime BookedAt,
    DateTime? ExpiresAt);

public record BookingSeatDto(
    Guid SeatId,
    string Row,
    int Number,
    string Category,
    decimal Price);

public record SeatAvailabilityDto(
    Guid SeatId,
    string Row,
    int Number,
    string Category,
    decimal Price,
    bool IsAvailable,
    bool IsLocked);

public record BookingHistoryDto(
    Guid Id,
    string BookingNumber,
    string MovieTitle,
    string? MoviePoster,
    string TheaterName,
    DateTime ShowTime,
    int TotalSeats,
    decimal TotalAmount,
    string Status,
    DateTime BookedAt);
