using ShowSphere.Domain.Enums;

namespace ShowSphere.Domain.Entities;

public class Movie : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public int DurationMinutes { get; set; }
    public string Language { get; set; } = string.Empty;
    public MovieCertificate Certificate { get; set; }
    public DateTime ReleaseDate { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }

    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
    public ICollection<MovieCast> MovieCasts { get; set; } = new List<MovieCast>();
    public ICollection<Show> Shows { get; set; } = new List<Show>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
