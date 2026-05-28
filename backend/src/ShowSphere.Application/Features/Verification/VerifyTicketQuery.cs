using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShowSphere.Application.Common;
using ShowSphere.Application.Features.Bookings.Handlers;
using ShowSphere.Domain.Enums;

namespace ShowSphere.Application.Features.Verification;

// ── Request ───────────────────────────────────────────────────────────────────
public record VerifyTicketQuery(string QRData) : IRequest<Result<TicketVerificationDto>>;

// ── DTOs ──────────────────────────────────────────────────────────────────────
public record TicketVerificationDto(
    string BookingNumber,
    string BookingStatus,
    string CustomerName,
    string CustomerEmail,
    string MovieTitle,
    string? PosterUrl,
    string TheaterName,
    string TheaterAddress,
    string City,
    string ScreenName,
    string ScreenType,
    DateTime ShowTime,
    List<VerifiedSeatDto> Seats,
    decimal TotalAmount,
    string PaymentMethod,
    string? TransactionId,
    DateTime BookedAt,
    bool IsScanned,
    DateTime? ScannedAt,
    /// <summary>Human-readable verdict shown to staff: VALID, ALREADY USED, or INVALID.</summary>
    string VerificationStatus,
    string VerificationMessage);

public record VerifiedSeatDto(string Row, int Number, string Category, decimal Price);

// ── Handler ───────────────────────────────────────────────────────────────────
public class VerifyTicketQueryHandler : IRequestHandler<VerifyTicketQuery, Result<TicketVerificationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly string _qrSecret;

    public VerifyTicketQueryHandler(IApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _qrSecret = configuration["QrCode:Secret"] ?? "ShowSphere-QR-HMAC-Secret-Key-2026";
    }

    public async Task<Result<TicketVerificationDto>> Handle(VerifyTicketQuery request, CancellationToken cancellationToken)
    {
        // ── 1. Parse QR payload ───────────────────────────────────────────────
        var parts = request.QRData?.Split('|');
        if (parts == null || parts.Length != 3 || parts[0] != "SHOWSPHERE")
            return Result<TicketVerificationDto>.Failure("Invalid QR code format.", 400);

        var bookingNumber = parts[1];
        var providedHmac  = parts[2];

        // ── 2. Verify HMAC signature ──────────────────────────────────────────
        var expectedHmac = ComputeHmac(bookingNumber, _qrSecret);

        bool signatureValid;
        try
        {
            var providedBytes = Convert.FromHexString(providedHmac);
            var expectedBytes = Convert.FromHexString(expectedHmac);
            signatureValid = CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
        }
        catch
        {
            signatureValid = false;
        }

        if (!signatureValid)
            return Result<TicketVerificationDto>.Failure("QR code signature is invalid. This ticket may have been tampered with.", 401);

        // ── 3. Fetch booking ──────────────────────────────────────────────────
        var booking = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat)
            .Include(b => b.Show).ThenInclude(s => s.Movie)
            .Include(b => b.Show).ThenInclude(s => s.Screen).ThenInclude(sc => sc.Theater)
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.BookingNumber == bookingNumber, cancellationToken);

        if (booking == null)
            return Result<TicketVerificationDto>.Failure("Booking not found in system.", 404);

        // ── 4. Determine verification result ─────────────────────────────────
        string verificationStatus;
        string verificationMessage;

        if (booking.Status != BookingStatus.Confirmed)
        {
            verificationStatus = "INVALID";
            verificationMessage = $"Ticket is not valid — booking status is '{booking.Status}'.";
        }
        else if (booking.IsScanned)
        {
            verificationStatus = "ALREADY_USED";
            verificationMessage = $"Ticket was already scanned on {booking.ScannedAt:dd MMM yyyy 'at' HH:mm}.";
        }
        else
        {
            // ── 5. Mark as scanned (first-time use) ───────────────────────────
            booking.IsScanned = true;
            booking.ScannedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            verificationStatus = "VALID";
            verificationMessage = "Ticket is valid — entry granted.";
        }

        var dto = new TicketVerificationDto(
            BookingNumber:      booking.BookingNumber,
            BookingStatus:      booking.Status.ToString(),
            CustomerName:       $"{booking.User.FirstName} {booking.User.LastName}",
            CustomerEmail:      booking.User.Email,
            MovieTitle:         booking.Show.Movie.Title,
            PosterUrl:          booking.Show.Movie.PosterUrl,
            TheaterName:        booking.Show.Screen.Theater.Name,
            TheaterAddress:     booking.Show.Screen.Theater.Address,
            City:               booking.Show.Screen.Theater.City,
            ScreenName:         booking.Show.Screen.Name,
            ScreenType:         booking.Show.Screen.ScreenType.ToString(),
            ShowTime:           booking.Show.StartTime,
            Seats:              booking.BookingSeats.Select(bs =>
                                    new VerifiedSeatDto(bs.Seat.Row, bs.Seat.Number,
                                        bs.Seat.Category.ToString(), bs.Price)).ToList(),
            TotalAmount:        booking.TotalAmount,
            PaymentMethod:      booking.Payment?.Method.ToString() ?? "Unknown",
            TransactionId:      booking.Payment?.TransactionId,
            BookedAt:           booking.CreatedAt,
            IsScanned:          booking.IsScanned,
            ScannedAt:          booking.ScannedAt,
            VerificationStatus: verificationStatus,
            VerificationMessage:verificationMessage);

        return Result<TicketVerificationDto>.Success(dto);
    }

    private static string ComputeHmac(string data, string secret)
    {
        var keyBytes  = Encoding.UTF8.GetBytes(secret);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        using var hmac = new HMACSHA256(keyBytes);
        return Convert.ToHexString(hmac.ComputeHash(dataBytes)).ToLowerInvariant();
    }
}
