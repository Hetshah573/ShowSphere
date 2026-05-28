namespace ShowSphere.Domain.Entities;

public class Genre
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
}

public class MovieGenre
{
    public Guid MovieId { get; set; }
    public int GenreId { get; set; }
    public Movie Movie { get; set; } = null!;
    public Genre Genre { get; set; } = null!;
}
