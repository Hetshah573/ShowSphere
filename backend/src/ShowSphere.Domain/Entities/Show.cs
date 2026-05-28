namespace ShowSphere.Domain.Entities;

public class Show : BaseEntity
{
    public Guid MovieId { get; set; }
    public Guid ScreenId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsActive { get; set; } = true;

    public Movie Movie { get; set; } = null!;
    public Screen Screen { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
