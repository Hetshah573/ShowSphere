namespace ShowSphere.Domain.Entities;

public class Review : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid MovieId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }

    public User User { get; set; } = null!;
    public Movie Movie { get; set; } = null!;
}
