using ShowSphere.Domain.Enums;

namespace ShowSphere.Domain.Entities;

public class BookingSeat
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookingId { get; set; }
    public Guid SeatId { get; set; }
    public decimal Price { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public Booking Booking { get; set; } = null!;
    public Seat Seat { get; set; } = null!;
}
