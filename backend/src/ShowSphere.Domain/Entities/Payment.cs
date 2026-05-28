using ShowSphere.Domain.Enums;

namespace ShowSphere.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid BookingId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public string? TransactionId { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime? PaidAt { get; set; }

    public Booking Booking { get; set; } = null!;
}
