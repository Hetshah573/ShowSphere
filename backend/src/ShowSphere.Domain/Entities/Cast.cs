namespace ShowSphere.Domain.Entities;

public class Cast : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public ICollection<MovieCast> MovieCasts { get; set; } = new List<MovieCast>();
}

public class MovieCast
{
    public Guid MovieId { get; set; }
    public Guid CastId { get; set; }
    public string Role { get; set; } = string.Empty;
    public Movie Movie { get; set; } = null!;
    public Cast Cast { get; set; } = null!;
}
