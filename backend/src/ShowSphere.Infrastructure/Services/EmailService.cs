using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShowSphere.Domain.Interfaces;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ShowSphere.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly string _senderName;

    public EmailService(HttpClient httpClient, ILogger<EmailService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["Email:BrevoApiKey"] ?? "";
        _fromEmail = configuration["Email:FromEmail"] ?? "hetshah11904@gmail.com";
        _senderName = configuration["Email:SenderName"] ?? "ShowSphere";
    }

    public async Task SendBookingConfirmationAsync(string toEmail, string userName, string bookingNumber, string movieTitle, string showTime, string seats, decimal amount, string? qrCodeBase64 = null)
    {
        var body = $@"
        <div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;background:#ffffff;padding:32px;border:1px solid #e5e7eb;border-radius:8px;'>
            <div style='text-align:center;margin-bottom:24px;border-bottom:1px solid #e5e7eb;padding-bottom:16px;'>
                <h1 style='color:#4f46e5;margin:0;font-size:24px;'>ShowSphere</h1>
                <p style='color:#6b7280;margin:4px 0 0;font-size:13px;'>Your Booking Confirmation</p>
            </div>
            <div style='padding:0 8px;'>
                <h2 style='color:#16a34a;margin-top:0;font-size:18px;'>Booking Confirmed</h2>
                <p>Dear {userName},</p>
                <p>Thank you for your booking. Please find the details below:</p>
                <table style='width:100%;border-collapse:collapse;margin:16px 0;'>
                    <tr><td style='padding:8px 0;color:#6b7280;'>Booking Reference</td><td style='padding:8px 0;font-weight:bold;'>{bookingNumber}</td></tr>
                    <tr><td style='padding:8px 0;color:#6b7280;'>Movie</td><td style='padding:8px 0;font-weight:bold;'>{movieTitle}</td></tr>
                    <tr><td style='padding:8px 0;color:#6b7280;'>Show Time</td><td style='padding:8px 0;font-weight:bold;'>{showTime}</td></tr>
                    <tr><td style='padding:8px 0;color:#6b7280;'>Seats</td><td style='padding:8px 0;font-weight:bold;'>{seats}</td></tr>
                    <tr><td style='padding:8px 0;color:#6b7280;'>Amount Paid</td><td style='padding:8px 0;font-weight:bold;'>INR {amount:N2}</td></tr>
                </table>
                <p style='color:#6b7280;font-size:13px;margin-top:16px;'>Your QR code is attached to this email. Please present it at the venue entrance.</p>
                <p style='color:#6b7280;font-size:13px;'>We recommend arriving at least 15 minutes before the showtime.</p>
            </div>
            <div style='text-align:center;margin-top:24px;border-top:1px solid #e5e7eb;padding-top:16px;'>
                <p style='color:#9ca3af;font-size:11px;margin:0;'>This is an automated message from ShowSphere. Please do not reply to this email.</p>
            </div>
        </div>";

        await SendEmailAsync(toEmail, $"Booking Confirmed - {movieTitle} | {bookingNumber}", body, qrCodeBase64);
    }

    public async Task SendBookingCancellationAsync(string toEmail, string userName, string bookingNumber, string movieTitle)
    {
        var body = $@"
        <div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;background:#ffffff;padding:32px;border:1px solid #e5e7eb;border-radius:8px;'>
            <div style='text-align:center;margin-bottom:24px;border-bottom:1px solid #e5e7eb;padding-bottom:16px;'>
                <h1 style='color:#4f46e5;margin:0;font-size:24px;'>ShowSphere</h1>
                <p style='color:#6b7280;margin:4px 0 0;font-size:13px;'>Booking Cancellation Notice</p>
            </div>
            <div style='padding:0 8px;'>
                <h2 style='color:#dc2626;margin-top:0;font-size:18px;'>Booking Cancelled</h2>
                <p>Dear {userName},</p>
                <p>Your booking has been cancelled as requested. Below are the details for your reference:</p>
                <table style='width:100%;border-collapse:collapse;margin:16px 0;'>
                    <tr><td style='padding:8px 0;color:#6b7280;'>Booking Reference</td><td style='padding:8px 0;font-weight:bold;'>{bookingNumber}</td></tr>
                    <tr><td style='padding:8px 0;color:#6b7280;'>Movie</td><td style='padding:8px 0;font-weight:bold;'>{movieTitle}</td></tr>
                </table>
                <p style='color:#374151;font-size:14px;'>If payment was made online, the refund will be processed to your original payment method within 5-7 business days.</p>
                <p style='color:#6b7280;font-size:13px;margin-top:16px;'>If you did not initiate this cancellation, please contact our support team immediately.</p>
            </div>
            <div style='text-align:center;margin-top:24px;border-top:1px solid #e5e7eb;padding-top:16px;'>
                <p style='color:#9ca3af;font-size:11px;margin:0;'>This is an automated message from ShowSphere. Please do not reply to this email.</p>
            </div>
        </div>";

        await SendEmailAsync(toEmail, $"Booking Cancellation - {movieTitle} | {bookingNumber}", body);
    }

    public async Task SendPasswordResetAsync(string toEmail, string userName, string resetToken)
    {
        var resetLink = $"http://localhost:5173/reset-password?token={resetToken}";
        var body = $@"
        <div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;background:#ffffff;padding:32px;border:1px solid #e5e7eb;border-radius:8px;'>
            <div style='text-align:center;margin-bottom:24px;border-bottom:1px solid #e5e7eb;padding-bottom:16px;'>
                <h1 style='color:#4f46e5;margin:0;font-size:24px;'>ShowSphere</h1>
                <p style='color:#6b7280;margin:4px 0 0;font-size:13px;'>Password Reset Request</p>
            </div>
            <div style='padding:0 8px;'>
                <h2 style='margin-top:0;font-size:18px;'>Password Reset</h2>
                <p>Dear {userName},</p>
                <p>We received a request to reset the password associated with your account. Please click the button below to proceed:</p>
                <div style='text-align:center;margin:24px 0;'>
                    <a href='{resetLink}' style='background:#4f46e5;color:white;padding:12px 32px;border-radius:6px;text-decoration:none;font-weight:600;display:inline-block;'>Reset Password</a>
                </div>
                <p style='color:#374151;font-size:14px;'>This link will expire in 1 hour for security purposes.</p>
                <p style='color:#6b7280;font-size:13px;'>If you did not request a password reset, no action is required and you may safely disregard this email.</p>
            </div>
            <div style='text-align:center;margin-top:24px;border-top:1px solid #e5e7eb;padding-top:16px;'>
                <p style='color:#9ca3af;font-size:11px;margin:0;'>This is an automated message from ShowSphere. Please do not reply to this email.</p>
            </div>
        </div>";

        await SendEmailAsync(toEmail, "Password Reset Request - ShowSphere", body);
    }

    public async Task SendMovieReleaseNotificationAsync(string toEmail, string userName, string movieTitle, string releaseDate)
    {
        var body = $@"
        <div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;background:#ffffff;padding:32px;border:1px solid #e5e7eb;border-radius:8px;'>
            <div style='text-align:center;margin-bottom:24px;border-bottom:1px solid #e5e7eb;padding-bottom:16px;'>
                <h1 style='color:#4f46e5;margin:0;font-size:24px;'>ShowSphere</h1>
                <p style='color:#6b7280;margin:4px 0 0;font-size:13px;'>Movie Release Notification</p>
            </div>
            <div style='padding:0 8px;'>
                <h2 style='color:#4f46e5;margin-top:0;font-size:18px;'>Now Showing: {movieTitle}</h2>
                <p>Dear {userName},</p>
                <p>We are pleased to inform you that <strong>{movieTitle}</strong> is now available for booking on ShowSphere.</p>
                <p style='color:#374151;'>Release Date: <strong>{releaseDate}</strong></p>
                <div style='text-align:center;margin:24px 0;'>
                    <a href='http://localhost:5173/movies' style='background:#4f46e5;color:white;padding:12px 32px;border-radius:6px;text-decoration:none;font-weight:600;display:inline-block;'>View Showtimes</a>
                </div>
                <p style='color:#6b7280;font-size:13px;'>You are receiving this email because you subscribed to release notifications for this movie.</p>
            </div>
            <div style='text-align:center;margin-top:24px;border-top:1px solid #e5e7eb;padding-top:16px;'>
                <p style='color:#9ca3af;font-size:11px;margin:0;'>This is an automated message from ShowSphere. Please do not reply to this email.</p>
            </div>
        </div>";

        await SendEmailAsync(toEmail, $"{movieTitle} - Now Available for Booking | ShowSphere", body);
    }

    public async Task SendSchedulerReportAsync(string toEmail, string userName, SchedulerReport report)
    {
        var statusColor = report.IsSuccess ? "#16a34a" : "#dc2626";
        var statusText = report.IsSuccess ? "Completed Successfully" : "Failed";
        var statusIcon = report.IsSuccess ? "&#10004;" : "&#10008;";

        var detailsTable = new StringBuilder();
        if (report.ShowDetails.Count > 0)
        {
            // Group by movie for cleaner presentation
            var grouped = report.ShowDetails
                .GroupBy(s => s.MovieTitle)
                .OrderBy(g => g.Key);

            foreach (var group in grouped)
            {
                detailsTable.Append($@"
                <tr style='background:#f3f4f6;'>
                    <td colspan='4' style='padding:10px 12px;font-weight:bold;color:#1f2937;border-bottom:1px solid #e5e7eb;'>{group.Key} ({group.Count()} shows)</td>
                </tr>");

                foreach (var show in group.OrderBy(s => s.StartTime))
                {
                    detailsTable.Append($@"
                    <tr>
                        <td style='padding:6px 12px;color:#374151;border-bottom:1px solid #f3f4f6;'>{show.TheaterName}</td>
                        <td style='padding:6px 12px;color:#374151;border-bottom:1px solid #f3f4f6;'>{show.ScreenName} ({show.ScreenType})</td>
                        <td style='padding:6px 12px;color:#374151;border-bottom:1px solid #f3f4f6;'>{show.StartTime:dd MMM, hh:mm tt}</td>
                        <td style='padding:6px 12px;color:#374151;border-bottom:1px solid #f3f4f6;text-align:right;'>&#8377;{show.BasePrice:N0}</td>
                    </tr>");
                }
            }
        }

        var errorSection = !report.IsSuccess
            ? $@"<div style='background:#fef2f2;border:1px solid #fecaca;border-radius:6px;padding:12px 16px;margin:16px 0;'>
                    <p style='color:#dc2626;font-weight:600;margin:0 0 4px;'>Error Details:</p>
                    <p style='color:#991b1b;margin:0;font-size:13px;font-family:monospace;'>{report.ErrorMessage}</p>
                </div>"
            : "";

        var showDetailsSection = report.ShowDetails.Count > 0
            ? $@"<div style='margin-top:20px;'>
                    <h3 style='color:#1f2937;font-size:15px;margin-bottom:8px;'>Shows Created Breakdown</h3>
                    <table style='width:100%;border-collapse:collapse;font-size:13px;border:1px solid #e5e7eb;border-radius:6px;'>
                        <thead>
                            <tr style='background:#4f46e5;color:white;'>
                                <th style='padding:8px 12px;text-align:left;'>Theater</th>
                                <th style='padding:8px 12px;text-align:left;'>Screen</th>
                                <th style='padding:8px 12px;text-align:left;'>Show Time</th>
                                <th style='padding:8px 12px;text-align:right;'>Base Price</th>
                            </tr>
                        </thead>
                        <tbody>
                            {detailsTable}
                        </tbody>
                    </table>
                </div>"
            : "<p style='color:#6b7280;font-style:italic;'>No new shows were needed — all slots already filled.</p>";

        var body = $@"
        <div style='font-family:Arial,sans-serif;max-width:700px;margin:0 auto;background:#ffffff;padding:32px;border:1px solid #e5e7eb;border-radius:8px;'>
            <div style='text-align:center;margin-bottom:24px;border-bottom:1px solid #e5e7eb;padding-bottom:16px;'>
                <h1 style='color:#4f46e5;margin:0;font-size:24px;'>ShowSphere</h1>
                <p style='color:#6b7280;margin:4px 0 0;font-size:13px;'>Automated Show Scheduler Report</p>
            </div>
            <div style='padding:0 8px;'>
                <div style='display:flex;align-items:center;gap:8px;margin-bottom:16px;'>
                    <span style='color:{statusColor};font-size:20px;'>{statusIcon}</span>
                    <h2 style='color:{statusColor};margin:0;font-size:18px;'>Scheduler {statusText}</h2>
                </div>
                <p>Dear {userName},</p>
                <p>The automated show scheduler ran at <strong>{report.RunTime:dd MMM yyyy, hh:mm tt} UTC</strong>. Here is the summary:</p>
                {errorSection}
                <table style='width:100%;border-collapse:collapse;margin:16px 0;background:#f9fafb;border-radius:6px;'>
                    <tr><td style='padding:10px 16px;color:#6b7280;border-bottom:1px solid #e5e7eb;'>Eligible Movies</td><td style='padding:10px 16px;font-weight:bold;border-bottom:1px solid #e5e7eb;'>{report.EligibleMovies}</td></tr>
                    <tr><td style='padding:10px 16px;color:#6b7280;border-bottom:1px solid #e5e7eb;'>Shows Created</td><td style='padding:10px 16px;font-weight:bold;color:#16a34a;border-bottom:1px solid #e5e7eb;'>{report.ShowsCreated}</td></tr>
                    <tr><td style='padding:10px 16px;color:#6b7280;border-bottom:1px solid #e5e7eb;'>Past Shows Deactivated (has bookings)</td><td style='padding:10px 16px;font-weight:bold;color:#d97706;border-bottom:1px solid #e5e7eb;'>{report.DeactivatedShows}</td></tr>
                    <tr><td style='padding:10px 16px;color:#6b7280;'>Past Shows Deleted (no bookings)</td><td style='padding:10px 16px;font-weight:bold;color:#dc2626;'>{report.DeletedShows}</td></tr>
                </table>
                {showDetailsSection}
            </div>
            <div style='text-align:center;margin-top:24px;border-top:1px solid #e5e7eb;padding-top:16px;'>
                <p style='color:#9ca3af;font-size:11px;margin:0;'>This is an automated report from ShowSphere Scheduler. Please do not reply to this email.</p>
            </div>
        </div>";

        var status = report.IsSuccess ? "Success" : "FAILED";
        await SendEmailAsync(toEmail, $"[ShowSphere Scheduler] {status} — {report.ShowsCreated} shows created | {report.RunTime:dd MMM yyyy}", body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody, string? qrCodeBase64 = null)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("[EMAIL] Brevo API key not configured. Skipping email to {Email}: {Subject}", toEmail, subject);
            return;
        }

        try
        {
            var payload = new Dictionary<string, object>
            {
                ["sender"] = new { name = _senderName, email = _fromEmail },
                ["to"] = new[] { new { email = toEmail } },
                ["subject"] = subject,
                ["htmlContent"] = htmlBody
            };

            // Attach QR code as a downloadable file
            if (!string.IsNullOrEmpty(qrCodeBase64))
            {
                var base64Data = qrCodeBase64.Contains(',')
                    ? qrCodeBase64.Substring(qrCodeBase64.IndexOf(',') + 1)
                    : qrCodeBase64;

                payload["attachment"] = new[] { new { content = base64Data, name = "booking-qrcode.png" } };
            }

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("api-key", _apiKey);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("[EMAIL] Sent successfully to {Email}: {Subject}", toEmail, subject);
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("[EMAIL] Brevo API error ({StatusCode}) to {Email}: {Error}", response.StatusCode, toEmail, errorBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EMAIL] Failed to send email to {Email}: {Subject}", toEmail, subject);
        }
    }
}
