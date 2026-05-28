using ShowSphere.Domain.Enums;

namespace ShowSphere.Domain.Entities;

public class Screen : BaseEntity
{
    public Guid TheaterId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
    public ScreenType ScreenType { get; set; }

    public Theater Theater { get; set; } = null!;
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public ICollection<Show> Shows { get; set; } = new List<Show>();
}
