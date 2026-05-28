using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShowSphere.Application.Common;
using ShowSphere.Domain.Entities;
using ShowSphere.Domain.Enums;
using ShowSphere.Domain.Interfaces;
using System.Security.Claims;

namespace ShowSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentGateway _paymentGateway;
    private readonly IApplicationDbContext _context;

    public PaymentsController(IPaymentGateway paymentGateway, IApplicationDbContext context)
    {
        _paymentGateway = paymentGateway;
        _context = context;
    }

    /// <summary>
    /// Creates a payment order on the gateway for a pending booking.
    /// Frontend uses the returned orderId to open checkout.
    /// </summary>
    [HttpPost("create-order")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var booking = await _context.Bookings
            .Include(b => b.Show).ThenInclude(s => s.Movie)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId && b.UserId == userId);

        if (booking == null)
            return NotFound(new { error = "Booking not found" });

        if (booking.Status != BookingStatus.Pending)
            return BadRequest(new { error = "Booking is not in pending state" });

        if (booking.ExpiresAt < DateTime.UtcNow)
            return Gone(new { error = "Booking has expired" });

        var user = await _context.Users.FindAsync(userId);

        var result = await _paymentGateway.CreateOrderAsync(new CreatePaymentOrderRequest(
            BookingNumber: booking.BookingNumber,
            Amount: booking.TotalAmount,
            Currency: "INR",
            CustomerEmail: user!.Email,
            CustomerName: $"{user.FirstName} {user.LastName}",
            Metadata: new Dictionary<string, string>
            {
                ["bookingId"] = booking.Id.ToString(),
                ["movie"] = booking.Show.Movie.Title
            }));

        if (!result.Success)
            return BadRequest(new { error = result.Error });

        return Ok(new
        {
            orderId = result.OrderId,
            amount = booking.TotalAmount,
            currency = "INR",
            gatewayKey = result.GatewayKey,
            provider = _paymentGateway.ProviderName,
            bookingNumber = booking.BookingNumber,
            customerName = $"{user.FirstName} {user.LastName}",
            customerEmail = user.Email
        });
    }

    /// <summary>
    /// Verifies payment signature after user completes checkout.
    /// On success, confirms the booking.
    /// </summary>
    [HttpPost("verify")]
    public async Task<IActionResult> VerifyPayment([FromBody] VerifyPaymentRequestDto request)
    {
        var verification = await _paymentGateway.VerifyPaymentAsync(new VerifyPaymentRequest(
            request.OrderId,
            request.PaymentId,
            request.Signature));

        if (!verification.IsValid)
            return BadRequest(new { error = verification.Error ?? "Payment verification failed" });

        return Ok(new
        {
            verified = true,
            transactionId = verification.TransactionId
        });
    }

    private ObjectResult Gone(object value) => StatusCode(410, value);
}

public record CreateOrderRequest(Guid BookingId);
public record VerifyPaymentRequestDto(string OrderId, string PaymentId, string Signature);
