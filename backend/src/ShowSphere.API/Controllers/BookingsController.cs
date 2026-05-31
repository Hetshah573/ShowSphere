using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ShowSphere.API.Hubs;
using ShowSphere.Application.Common;
using ShowSphere.Application.Features.Bookings.Commands;
using ShowSphere.Application.Features.Bookings.DTOs;
using ShowSphere.Application.Features.Bookings.Queries;

namespace ShowSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHubContext<SeatHub> _seatHub;
    private readonly IApplicationDbContext _context;

    public BookingsController(IMediator mediator, IHubContext<SeatHub> seatHub, IApplicationDbContext context)
    {
        _mediator = mediator;
        _seatHub = seatHub;
        _context = context;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("seats/{showId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSeatAvailability(Guid showId)
    {
        var result = await _mediator.Send(new GetSeatAvailabilityQuery(showId));
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
    {
        var command = new CreateBookingCommand(GetUserId(), request.ShowId, request.SeatIds, request.PaymentMethod);
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        // Pending booking locks selected seats for this show.
        if (result.Data != null)
        {
            await BroadcastSeatUpdatesAsync(
                request.ShowId,
                result.Data.Seats.Select(s => s.SeatId),
                isAvailable: false,
                isLocked: true);
        }

        return StatusCode(result.StatusCode, result.Data);
    }

    [HttpPost("{bookingId:guid}/confirm")]
    public async Task<IActionResult> ConfirmPayment(Guid bookingId, [FromBody] ConfirmPaymentRequest request)
    {
        var bookingSnapshot = await _context.Bookings
            .Include(b => b.BookingSeats)
            .Where(b => b.Id == bookingId && b.UserId == GetUserId())
            .Select(b => new
            {
                b.ShowId,
                SeatIds = b.BookingSeats.Select(bs => bs.SeatId).ToList()
            })
            .FirstOrDefaultAsync();

        var command = new ConfirmPaymentCommand(bookingId, GetUserId(), request.TransactionId);
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        // Confirmed seats remain unavailable and are no longer just lock-held.
        if (bookingSnapshot != null)
        {
            await BroadcastSeatUpdatesAsync(
                bookingSnapshot.ShowId,
                bookingSnapshot.SeatIds,
                isAvailable: false,
                isLocked: false);
        }

        return Ok(result.Data);
    }

    [HttpPost("{bookingId:guid}/cancel")]
    public async Task<IActionResult> CancelBooking(Guid bookingId)
    {
        // Snapshot seat ids/show id before cancellation so we can emit realtime release events.
        var bookingSnapshot = await _context.Bookings
            .Include(b => b.BookingSeats)
            .Where(b => b.Id == bookingId && b.UserId == GetUserId())
            .Select(b => new
            {
                b.ShowId,
                SeatIds = b.BookingSeats.Select(bs => bs.SeatId).ToList()
            })
            .FirstOrDefaultAsync();

        var command = new CancelBookingCommand(bookingId, GetUserId());
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        if (bookingSnapshot != null)
        {
            await BroadcastSeatUpdatesAsync(
                bookingSnapshot.ShowId,
                bookingSnapshot.SeatIds,
                isAvailable: true,
                isLocked: false);
        }

        return Ok(new { message = "Booking cancelled successfully" });
    }

    [HttpGet("{bookingId:guid}")]
    public async Task<IActionResult> GetBooking(Guid bookingId)
    {
        var result = await _mediator.Send(new GetBookingByIdQuery(bookingId, GetUserId()));
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(result.Data);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetBookingHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetBookingHistoryQuery(GetUserId(), page, pageSize));
        return Ok(result.Data);
    }

    private async Task BroadcastSeatUpdatesAsync(Guid showId, IEnumerable<Guid> seatIds, bool isAvailable, bool isLocked)
    {
        var tasks = seatIds.Select(seatId =>
            _seatHub.Clients.Group($"show_{showId}")
                .SendAsync("SeatUpdated", seatId.ToString(), isAvailable, isLocked));

        await Task.WhenAll(tasks);
    }
}

public record ConfirmPaymentRequest(string TransactionId);
