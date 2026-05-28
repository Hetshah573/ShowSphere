using ShowSphere.Domain.Enums;

namespace ShowSphere.Domain.Entities;

public class Booking : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ShowId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
    public decimal TotalAmount { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public DateTime? ExpiresAt { get; set; }
    public string? QRCode { get; set; }
    public bool IsScanned { get; set; } = false;
    public DateTime? ScannedAt { get; set; }
    public uint RowVersion { get; set; } // EF Core concurrency token — maps to xmin in PostgreSQL

    public User User { get; set; } = null!;
    public Show Show { get; set; } = null!;
    public Payment? Payment { get; set; }
    public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
}
