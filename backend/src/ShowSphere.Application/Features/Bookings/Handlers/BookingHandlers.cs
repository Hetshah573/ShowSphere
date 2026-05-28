using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShowSphere.Application.Common;
using ShowSphere.Application.Common.Exceptions;
using ShowSphere.Application.Features.Bookings.Commands;
using ShowSphere.Application.Features.Bookings.DTOs;
using ShowSphere.Application.Features.Bookings.Queries;
using ShowSphere.Domain.Entities;
using ShowSphere.Domain.Enums;
using ShowSphere.Domain.Interfaces;

namespace ShowSphere.Application.Features.Bookings.Handlers;

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, Result<BookingDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public CreateBookingCommandHandler(IApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<Result<BookingDto>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        // Verify user exists
        var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
            return Result<BookingDto>.Failure("User session expired. Please log in again.", 401);

        var show = await _context.Shows
            .Include(s => s.Movie)
            .Include(s => s.Screen).ThenInclude(s => s.Theater)
            .FirstOrDefaultAsync(s => s.Id == request.ShowId && s.IsActive, cancellationToken);

        if (show == null)
            return Result<BookingDto>.Failure("Show not found", 404);

        if (show.StartTime < DateTime.UtcNow)
            return Result<BookingDto>.Failure("Cannot book for past shows", 400);

        // Check seat availability with locking
        var seats = await _context.Seats
            .Where(s => request.SeatIds.Contains(s.Id) && s.ScreenId == show.ScreenId && s.IsActive)
            .ToListAsync(cancellationToken);

        if (seats.Count != request.SeatIds.Count)
            return Result<BookingDto>.Failure("One or more selected seats are invalid", 400);

        // Check for existing bookings/locks on these seats
        var lockedSeatIds = await _context.BookingSeats
            .Where(bs => request.SeatIds.Contains(bs.SeatId)
                && bs.Booking.ShowId == request.ShowId
                && (bs.Status == BookingStatus.Confirmed
                    || (bs.Status == BookingStatus.Pending && bs.Booking.ExpiresAt > DateTime.UtcNow)))
            .Select(bs => bs.SeatId)
            .ToListAsync(cancellationToken);

        if (lockedSeatIds.Any())
            return Result<BookingDto>.Failure("One or more seats are already booked or locked", 409);

        // Calculate total
        var totalAmount = seats.Sum(s => s.Price);

        // Create booking with expiration (10 min lock)
        var booking = new Booking
        {
            UserId = request.UserId,
            ShowId = request.ShowId,
            BookingNumber = GenerateBookingNumber(),
            TotalSeats = seats.Count,
            TotalAmount = totalAmount,
            Status = BookingStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        foreach (var seat in seats)
        {
            booking.BookingSeats.Add(new BookingSeat
            {
                BookingId = booking.Id,
                SeatId = seat.Id,
                Price = seat.Price,
                Status = BookingStatus.Pending
            });
        }

        // Create payment record
        var payment = new Payment
        {
            BookingId = booking.Id,
            Amount = totalAmount,
            Method = request.PaymentMethod,
            Status = PaymentStatus.Pending
        };

        _context.Bookings.Add(booking);
        _context.Payments.Add(payment);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<BookingDto>.Failure("Seats were booked by another user. Please try again.", 409);
        }

        await _auditService.LogAsync(request.UserId, "CreateBooking", "Booking", booking.Id.ToString(),
            $"Seats: {string.Join(",", seats.Select(s => $"{s.Row}{s.Number}"))}");

        var bookingDto = new BookingDto(
            booking.Id, booking.BookingNumber, show.Movie.Title,
            show.Screen.Theater.Name, show.Screen.Name, show.StartTime,
            booking.TotalSeats, booking.TotalAmount, booking.Status.ToString(),
            seats.Select(s => new BookingSeatDto(s.Id, s.Row, s.Number, s.Category.ToString(), s.Price)).ToList(),
            null, booking.CreatedAt, booking.ExpiresAt);

        return Result<BookingDto>.Success(bookingDto, 201);
    }

    private static string GenerateBookingNumber()
    {
        return $"SS{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }
}

public class ConfirmPaymentCommandHandler : IRequestHandler<ConfirmPaymentCommand, Result<BookingDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly IEmailService _emailService;
    private readonly string _qrSecret;

    public ConfirmPaymentCommandHandler(IApplicationDbContext context, IAuditService auditService, IEmailService emailService, IConfiguration configuration)
    {
        _context = context;
        _auditService = auditService;
        _emailService = emailService;
        _qrSecret = configuration["QrCode:Secret"] ?? "ShowSphere-QR-HMAC-Secret-Key-2026";
    }

    public async Task<Result<BookingDto>> Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        var booking = await _context.Bookings
            .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat)
            .Include(b => b.Show).ThenInclude(s => s.Movie)
            .Include(b => b.Show).ThenInclude(s => s.Screen).ThenInclude(sc => sc.Theater)
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId && b.UserId == request.UserId, cancellationToken);

        if (booking == null)
            return Result<BookingDto>.Failure("Booking not found", 404);

        if (booking.Status != BookingStatus.Pending)
            return Result<BookingDto>.Failure("Booking is not in pending state", 400);

        if (booking.ExpiresAt < DateTime.UtcNow)
        {
            booking.Status = BookingStatus.Expired;
            foreach (var bs in booking.BookingSeats) bs.Status = BookingStatus.Expired;
            await _context.SaveChangesAsync(cancellationToken);
            return Result<BookingDto>.Failure("Booking has expired. Please try again.", 410);
        }

        // Confirm booking
        booking.Status = BookingStatus.Confirmed;
        // QRCode stores the signed text (SHOWSPHERE|<bookingNumber>|<hmac>)
        // This same text is what gets QR-encoded everywhere (screen, email, PDF)
        booking.QRCode = GenerateQRCodeText(booking.BookingNumber, _qrSecret);
        foreach (var bs in booking.BookingSeats) bs.Status = BookingStatus.Confirmed;

        // Confirm payment
        if (booking.Payment != null)
        {
            booking.Payment.Status = PaymentStatus.Completed;
            booking.Payment.TransactionId = request.TransactionId;
            booking.Payment.PaidAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(booking.UserId, "ConfirmPayment", "Booking", booking.Id.ToString());

        // Send booking confirmation email — generate a PNG from the signed text for the inline image
        var user = await _context.Users.FindAsync(new object[] { booking.UserId }, cancellationToken);
        if (user != null)
        {
            var seatList = string.Join(", ", booking.BookingSeats.Select(bs => $"{bs.Seat.Row}{bs.Seat.Number}"));
            var qrPngBase64 = GenerateQRCodePngBase64(booking.QRCode!);
            await _emailService.SendBookingConfirmationAsync(
                user.Email, $"{user.FirstName} {user.LastName}",
                booking.BookingNumber, booking.Show.Movie.Title,
                booking.Show.StartTime.ToString("dd MMM yyyy, hh:mm tt"),
                seatList, booking.TotalAmount, qrPngBase64);
        }

        var dto = new BookingDto(
            booking.Id, booking.BookingNumber, booking.Show.Movie.Title,
            booking.Show.Screen.Theater.Name, booking.Show.Screen.Name,
            booking.Show.StartTime, booking.TotalSeats, booking.TotalAmount,
            booking.Status.ToString(),
            booking.BookingSeats.Select(bs => new BookingSeatDto(
                bs.SeatId, bs.Seat.Row, bs.Seat.Number, bs.Seat.Category.ToString(), bs.Price)).ToList(),
            booking.QRCode, booking.CreatedAt, booking.ExpiresAt);

        return Result<BookingDto>.Success(dto);
    }

    /// <summary>
    /// Returns the signed text that is QR-encoded: SHOWSPHERE|{bookingNumber}|{hmac}
    /// Stored in Booking.QRCode and used as the value in QRCodeSVG on the frontend.
    /// </summary>
    internal static string GenerateQRCodeText(string bookingNumber, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var dataBytes = Encoding.UTF8.GetBytes(bookingNumber);
        using var hmac = new HMACSHA256(keyBytes);
        var hashHex = Convert.ToHexString(hmac.ComputeHash(dataBytes)).ToLowerInvariant();
        return $"SHOWSPHERE|{bookingNumber}|{hashHex}";
    }

    /// <summary>
    /// Generates a PNG QR code from the signed text (used only for email inline image).
    /// </summary>
    private static string GenerateQRCodePngBase64(string qrText)
    {
        using var qrGenerator = new QRCoder.QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(qrText, QRCoder.QRCodeGenerator.ECCLevel.M);
        using var qrCode = new QRCoder.PngByteQRCode(qrCodeData);
        var pngBytes = qrCode.GetGraphic(8);
        return Convert.ToBase64String(pngBytes);
    }
}

public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly IEmailService _emailService;

    public CancelBookingCommandHandler(IApplicationDbContext context, IAuditService auditService, IEmailService emailService)
    {
        _context = context;
        _auditService = auditService;
        _emailService = emailService;
    }

    public async Task<Result<bool>> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _context.Bookings
            .Include(b => b.BookingSeats)
            .Include(b => b.Payment)
            .Include(b => b.Show).ThenInclude(s => s.Movie)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId && b.UserId == request.UserId, cancellationToken);

        if (booking == null)
            return Result<bool>.Failure("Booking not found", 404);

        if (booking.Status == BookingStatus.Cancelled)
            return Result<bool>.Failure("Booking is already cancelled", 400);

        if (booking.Show.StartTime < DateTime.UtcNow.AddHours(2))
            return Result<bool>.Failure("Cannot cancel booking less than 2 hours before show time", 400);

        booking.Status = BookingStatus.Cancelled;
        foreach (var bs in booking.BookingSeats) bs.Status = BookingStatus.Cancelled;

        if (booking.Payment?.Status == PaymentStatus.Completed)
            booking.Payment.Status = PaymentStatus.Refunded;

        await _context.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(request.UserId, "CancelBooking", "Booking", booking.Id.ToString());

        // Send cancellation email
        var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user != null)
        {
            await _emailService.SendBookingCancellationAsync(
                user.Email, $"{user.FirstName} {user.LastName}",
                booking.BookingNumber, booking.Show.Movie.Title);
        }

        return Result<bool>.Success(true);
    }
}

public class GetSeatAvailabilityQueryHandler : IRequestHandler<GetSeatAvailabilityQuery, Result<List<SeatAvailabilityDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetSeatAvailabilityQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<SeatAvailabilityDto>>> Handle(GetSeatAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var show = await _context.Shows
            .Include(s => s.Screen)
            .FirstOrDefaultAsync(s => s.Id == request.ShowId, cancellationToken);

        if (show == null)
            return Result<List<SeatAvailabilityDto>>.Failure("Show not found", 404);

        var seats = await _context.Seats
            .Where(s => s.ScreenId == show.ScreenId && s.IsActive)
            .OrderBy(s => s.Row).ThenBy(s => s.Number)
            .ToListAsync(cancellationToken);

        var bookedSeatIds = await _context.BookingSeats
            .Where(bs => bs.Booking.ShowId == request.ShowId
                && (bs.Status == BookingStatus.Confirmed
                    || (bs.Status == BookingStatus.Pending && bs.Booking.ExpiresAt > DateTime.UtcNow)))
            .Select(bs => bs.SeatId)
            .ToListAsync(cancellationToken);

        var lockedSeatIds = await _context.BookingSeats
            .Where(bs => bs.Booking.ShowId == request.ShowId
                && bs.Status == BookingStatus.Pending
                && bs.Booking.ExpiresAt > DateTime.UtcNow)
            .Select(bs => bs.SeatId)
            .ToListAsync(cancellationToken);

        var seatDtos = seats.Select(s => new SeatAvailabilityDto(
            s.Id, s.Row, s.Number, s.Category.ToString(), s.Price,
            !bookedSeatIds.Contains(s.Id),
            lockedSeatIds.Contains(s.Id))).ToList();

        return Result<List<SeatAvailabilityDto>>.Success(seatDtos);
    }
}

public class GetBookingByIdQueryHandler : IRequestHandler<GetBookingByIdQuery, Result<BookingDto>>
{
    private readonly IApplicationDbContext _context;

    public GetBookingByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<BookingDto>> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        var booking = await _context.Bookings
            .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat)
            .Include(b => b.Show).ThenInclude(s => s.Movie)
            .Include(b => b.Show).ThenInclude(s => s.Screen).ThenInclude(sc => sc.Theater)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId && b.UserId == request.UserId, cancellationToken);

        if (booking == null)
            return Result<BookingDto>.Failure("Booking not found", 404);

        // Lazy-expire: if booking is still Pending but time has passed, mark it expired now
        if (booking.Status == BookingStatus.Pending && booking.ExpiresAt < DateTime.UtcNow)
        {
            booking.Status = BookingStatus.Expired;
            foreach (var bs in booking.BookingSeats) bs.Status = BookingStatus.Expired;
            await _context.SaveChangesAsync(cancellationToken);
        }

        var dto = new BookingDto(
            booking.Id, booking.BookingNumber, booking.Show.Movie.Title,
            booking.Show.Screen.Theater.Name, booking.Show.Screen.Name,
            booking.Show.StartTime, booking.TotalSeats, booking.TotalAmount,
            booking.Status.ToString(),
            booking.BookingSeats.Select(bs => new BookingSeatDto(
                bs.SeatId, bs.Seat.Row, bs.Seat.Number, bs.Seat.Category.ToString(), bs.Price)).ToList(),
            booking.QRCode, booking.CreatedAt, booking.ExpiresAt);

        return Result<BookingDto>.Success(dto);
    }
}

public class GetBookingHistoryQueryHandler : IRequestHandler<GetBookingHistoryQuery, Result<PagedResult<BookingHistoryDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetBookingHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<BookingHistoryDto>>> Handle(GetBookingHistoryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Bookings
            .Include(b => b.Show).ThenInclude(s => s.Movie)
            .Include(b => b.Show).ThenInclude(s => s.Screen).ThenInclude(sc => sc.Theater)
            .Where(b => b.UserId == request.UserId)
            .OrderByDescending(b => b.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var bookings = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => new BookingHistoryDto(
                b.Id, b.BookingNumber, b.Show.Movie.Title, b.Show.Movie.PosterUrl,
                b.Show.Screen.Theater.Name, b.Show.StartTime,
                b.TotalSeats, b.TotalAmount, b.Status.ToString(), b.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<PagedResult<BookingHistoryDto>>.Success(new PagedResult<BookingHistoryDto>
        {
            Items = bookings,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}
