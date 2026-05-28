using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShowSphere.Application.Features.Verification;

namespace ShowSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VerificationController : ControllerBase
{
    private readonly IMediator _mediator;

    public VerificationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Validates a scanned QR code and returns full booking details.
    /// Accessible to admin users (staff scanning at venue).
    /// Marks the ticket as used on first successful scan.
    /// </summary>
    [HttpPost("scan")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ScanTicket([FromBody] ScanTicketRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.QRData))
            return BadRequest(new { error = "QR data is required." });

        var result = await _mediator.Send(new VerifyTicketQuery(request.QRData));

        return result.IsSuccess
            ? Ok(result.Data)
            : StatusCode(result.StatusCode, new { error = result.Error });
    }
}

public record ScanTicketRequest(string QRData);
