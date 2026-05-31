using Microsoft.EntityFrameworkCore;
using ShowSphere.Domain.Entities;

namespace ShowSphere.Application.Common;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Movie> Movies { get; }
    DbSet<Genre> Genres { get; }
    DbSet<Cast> Casts { get; }
    DbSet<MovieGenre> MovieGenres { get; }
    DbSet<MovieCast> MovieCasts { get; }
    DbSet<Theater> Theaters { get; }
    DbSet<Screen> Screens { get; }
    DbSet<Seat> Seats { get; }
    DbSet<Show> Shows { get; }
    DbSet<Booking> Bookings { get; }
    DbSet<BookingSeat> BookingSeats { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Review> Reviews { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Wishlist> Wishlists { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }
    DbSet<MovieNotificationSubscription> MovieNotificationSubscriptions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade Database { get; }
}
