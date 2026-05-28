using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public BookingsController(IMediator mediator)
    {
        _mediator = mediator;
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
        return StatusCode(result.StatusCode, result.Data);
    }

    [HttpPost("{bookingId:guid}/confirm")]
    public async Task<IActionResult> ConfirmPayment(Guid bookingId, [FromBody] ConfirmPaymentRequest request)
    {
        var command = new ConfirmPaymentCommand(bookingId, GetUserId(), request.TransactionId);
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(result.Data);
    }

    [HttpPost("{bookingId:guid}/cancel")]
    public async Task<IActionResult> CancelBooking(Guid bookingId)
    {
        var command = new CancelBookingCommand(bookingId, GetUserId());
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });
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
}

public record ConfirmPaymentRequest(string TransactionId);
