namespace ShowSphere.Domain.Entities;

public class Wishlist : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid MovieId { get; set; }

    public User User { get; set; } = null!;
    public Movie Movie { get; set; } = null!;
}
