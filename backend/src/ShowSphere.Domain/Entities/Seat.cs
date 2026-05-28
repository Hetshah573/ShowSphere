using ShowSphere.Domain.Enums;

namespace ShowSphere.Domain.Entities;

public class Seat : BaseEntity
{
    public Guid ScreenId { get; set; }
    public string Row { get; set; } = string.Empty;
    public int Number { get; set; }
    public SeatCategory Category { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;

    public Screen Screen { get; set; } = null!;
    public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
}
