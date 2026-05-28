using Microsoft.EntityFrameworkCore;
using ShowSphere.Application.Common;
using ShowSphere.Domain.Entities;

namespace ShowSphere.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // PostgreSQL (Npgsql) maps DateTime to timestamptz natively and always
        // returns DateTimeKind.Utc — no value converters needed.
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Cast> Casts => Set<Cast>();
    public DbSet<MovieGenre> MovieGenres => Set<MovieGenre>();
    public DbSet<MovieCast> MovieCasts => Set<MovieCast>();
    public DbSet<Theater> Theaters => Set<Theater>();
    public DbSet<Screen> Screens => Set<Screen>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Show> Shows => Set<Show>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingSeat> BookingSeats => Set<BookingSeat>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<MovieNotificationSubscription> MovieNotificationSubscriptions => Set<MovieNotificationSubscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.Phone).HasMaxLength(20);
            entity.HasOne(u => u.Role).WithMany(r => r.Users).HasForeignKey(u => u.RoleId);
        });

        // Role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(r => r.Name).HasMaxLength(50).IsRequired();
            entity.HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "User" }
            );
        });

        // RefreshToken
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(t => t.Token).IsUnique();
            entity.HasOne(t => t.User).WithMany(u => u.RefreshTokens).HasForeignKey(t => t.UserId);
        });

        // Movie
        modelBuilder.Entity<Movie>(entity =>
        {
            entity.Property(m => m.Title).HasMaxLength(300).IsRequired();
            entity.Property(m => m.Language).HasMaxLength(50);
            entity.Property(m => m.AverageRating).HasPrecision(3, 2);
        });

        // Genre
        modelBuilder.Entity<Genre>(entity =>
        {
            entity.Property(g => g.Name).HasMaxLength(50).IsRequired();
        });

        // MovieGenre
        modelBuilder.Entity<MovieGenre>(entity =>
        {
            entity.HasKey(mg => new { mg.MovieId, mg.GenreId });
            entity.HasOne(mg => mg.Movie).WithMany(m => m.MovieGenres).HasForeignKey(mg => mg.MovieId);
            entity.HasOne(mg => mg.Genre).WithMany(g => g.MovieGenres).HasForeignKey(mg => mg.GenreId);
        });

        // Cast
        modelBuilder.Entity<Cast>(entity =>
        {
            entity.Property(c => c.Name).HasMaxLength(200).IsRequired();
        });

        // MovieCast
        modelBuilder.Entity<MovieCast>(entity =>
        {
            entity.HasKey(mc => new { mc.MovieId, mc.CastId });
            entity.Property(mc => mc.Role).HasMaxLength(100);
            entity.HasOne(mc => mc.Movie).WithMany(m => m.MovieCasts).HasForeignKey(mc => mc.MovieId);
            entity.HasOne(mc => mc.Cast).WithMany(c => c.MovieCasts).HasForeignKey(mc => mc.CastId);
        });

        // Theater
        modelBuilder.Entity<Theater>(entity =>
        {
            entity.Property(t => t.Name).HasMaxLength(200).IsRequired();
            entity.Property(t => t.Address).HasMaxLength(500).IsRequired();
            entity.Property(t => t.City).HasMaxLength(100).IsRequired();
            entity.Property(t => t.State).HasMaxLength(100).IsRequired();
            entity.Property(t => t.PinCode).HasMaxLength(10).IsRequired();
            entity.Property(t => t.Latitude).HasPrecision(9, 6);
            entity.Property(t => t.Longitude).HasPrecision(9, 6);
        });

        // Screen
        modelBuilder.Entity<Screen>(entity =>
        {
            entity.Property(s => s.Name).HasMaxLength(50).IsRequired();
            entity.HasOne(s => s.Theater).WithMany(t => t.Screens).HasForeignKey(s => s.TheaterId);
        });

        // Seat
        modelBuilder.Entity<Seat>(entity =>
        {
            entity.Property(s => s.Row).HasMaxLength(5).IsRequired();
            entity.Property(s => s.Price).HasPrecision(10, 2);
            entity.HasOne(s => s.Screen).WithMany(sc => sc.Seats).HasForeignKey(s => s.ScreenId);
            entity.HasIndex(s => new { s.ScreenId, s.Row, s.Number }).IsUnique();
        });

        // Show
        modelBuilder.Entity<Show>(entity =>
        {
            entity.HasOne(s => s.Movie).WithMany(m => m.Shows).HasForeignKey(s => s.MovieId);
            entity.HasOne(s => s.Screen).WithMany(sc => sc.Shows).HasForeignKey(s => s.ScreenId);
            entity.Property(s => s.BasePrice).HasPrecision(10, 2);
        });

        // Booking
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasIndex(b => b.BookingNumber).IsUnique();
            entity.Property(b => b.BookingNumber).HasMaxLength(20).IsRequired();
            entity.Property(b => b.TotalAmount).HasPrecision(12, 2);
            entity.Property(b => b.RowVersion).IsRowVersion();
            entity.HasOne(b => b.User).WithMany(u => u.Bookings).HasForeignKey(b => b.UserId);
            entity.HasOne(b => b.Show).WithMany(s => s.Bookings).HasForeignKey(b => b.ShowId);
        });

        // BookingSeat
        modelBuilder.Entity<BookingSeat>(entity =>
        {
            entity.Property(bs => bs.Price).HasPrecision(10, 2);
            entity.HasOne(bs => bs.Booking).WithMany(b => b.BookingSeats).HasForeignKey(bs => bs.BookingId);
            entity.HasOne(bs => bs.Seat).WithMany(s => s.BookingSeats).HasForeignKey(bs => bs.SeatId);
        });

        // Payment
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(p => p.Amount).HasPrecision(12, 2);
            entity.Property(p => p.TransactionId).HasMaxLength(100);
            entity.HasOne(p => p.Booking).WithOne(b => b.Payment).HasForeignKey<Payment>(p => p.BookingId);
        });

        // Review
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasIndex(r => new { r.UserId, r.MovieId }).IsUnique();
            entity.Property(r => r.Comment).HasMaxLength(1000);
            entity.HasOne(r => r.User).WithMany(u => u.Reviews).HasForeignKey(r => r.UserId);
            entity.HasOne(r => r.Movie).WithMany(m => m.Reviews).HasForeignKey(r => r.MovieId);
        });

        // AuditLog
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.Property(a => a.Action).HasMaxLength(100).IsRequired();
            entity.Property(a => a.Entity).HasMaxLength(100).IsRequired();
            entity.Property(a => a.EntityId).HasMaxLength(100);
            entity.Property(a => a.Details).HasMaxLength(2000);
            entity.HasIndex(a => a.Timestamp);
        });

        // Wishlist
        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.HasIndex(w => new { w.UserId, w.MovieId }).IsUnique();
            entity.HasOne(w => w.User).WithMany(u => u.Wishlists).HasForeignKey(w => w.UserId);
            entity.HasOne(w => w.Movie).WithMany(m => m.Wishlists).HasForeignKey(w => w.MovieId);
        });

        // PasswordResetToken
        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasIndex(t => t.Token).IsUnique();
            entity.Property(t => t.Token).HasMaxLength(200).IsRequired();
            entity.HasOne(t => t.User).WithMany(u => u.PasswordResetTokens).HasForeignKey(t => t.UserId);
        });

        // MovieNotificationSubscription
        modelBuilder.Entity<MovieNotificationSubscription>(entity =>
        {
            entity.HasIndex(s => new { s.UserId, s.MovieId }).IsUnique();
            entity.HasOne(s => s.User).WithMany().HasForeignKey(s => s.UserId);
            entity.HasOne(s => s.Movie).WithMany().HasForeignKey(s => s.MovieId);
        });
    }
}
