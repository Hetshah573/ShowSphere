namespace ShowSphere.Domain.Entities;

public class MovieNotificationSubscription : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid MovieId { get; set; }
    public bool IsNotified { get; set; }

    public User User { get; set; } = null!;
    public Movie Movie { get; set; } = null!;
}
