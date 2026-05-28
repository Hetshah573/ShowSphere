using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShowSphere.Domain.Interfaces;

namespace ShowSphere.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _senderEmail;
    private readonly string _senderPassword;
    private readonly string _senderName;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _smtpHost = configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "587");
        _senderEmail = configuration["Email:SenderEmail"] ?? "";
        _senderPassword = configuration["Email:SenderPassword"] ?? "";
        _senderName = configuration["Email:SenderName"] ?? "ShowSphere";
    }

    public async Task SendBookingConfirmationAsync(string toEmail, string userName, string bookingNumber, string movieTitle, string showTime, string seats, decimal amount, string? qrCodeBase64 = null)
    {
        // Determine if QR data is a valid base64 image (has data URI prefix or is pure base64)
        var isValidQrImage = qrCodeBase64 != null && (qrCodeBase64.StartsWith("data:image") || IsBase64String(qrCodeBase64));

        var qrSection = isValidQrImage
            ? "<div style='text-align:center;margin:20px 0;'><p style='color:#6b7280;margin-bottom:8px;font-size:13px;'>Please present this QR code at the venue entrance:</p><img src='cid:qrcode' width='160' height='160' style='border:1px solid #e5e7eb;border-radius:4px;padding:8px;' /></div>"
            : "";

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
                {qrSection}
                <p style='color:#6b7280;font-size:13px;margin-top:16px;'>We recommend arriving at least 15 minutes before the showtime.</p>
            </div>
            <div style='text-align:center;margin-top:24px;border-top:1px solid #e5e7eb;padding-top:16px;'>
                <p style='color:#9ca3af;font-size:11px;margin:0;'>This is an automated message from ShowSphere. Please do not reply to this email.</p>
            </div>
        </div>";

        await SendEmailAsync(toEmail, $"Booking Confirmed - {movieTitle} | {bookingNumber}", body, isValidQrImage ? qrCodeBase64 : null);
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

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody, string? qrCodeBase64 = null)
    {
        if (string.IsNullOrEmpty(_senderEmail) || string.IsNullOrEmpty(_senderPassword))
        {
            _logger.LogWarning("[EMAIL] SMTP not configured. Skipping email to {Email}: {Subject}", toEmail, subject);
            return;
        }

        try
        {
            using var message = new MailMessage();
            message.From = new MailAddress(_senderEmail, _senderName);
            message.To.Add(new MailAddress(toEmail));
            message.Subject = subject;
            message.IsBodyHtml = true;

            if (qrCodeBase64 != null)
            {
                // Strip data URI prefix if present (e.g. "data:image/png;base64,")
                var base64Data = qrCodeBase64.Contains(',')
                    ? qrCodeBase64.Substring(qrCodeBase64.IndexOf(',') + 1)
                    : qrCodeBase64;

                // Only attach as image if it's valid base64; otherwise skip the inline image
                byte[]? qrBytes = null;
                try
                {
                    qrBytes = Convert.FromBase64String(base64Data);
                }
                catch (FormatException)
                {
                    _logger.LogWarning("[EMAIL] QR code data is not valid base64, skipping inline image attachment.");
                }

                if (qrBytes != null)
                {
                    var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html");
                    var qrStream = new MemoryStream(qrBytes);
                    var qrResource = new LinkedResource(qrStream, "image/png") { ContentId = "qrcode" };
                    htmlView.LinkedResources.Add(qrResource);
                    message.AlternateViews.Add(htmlView);
                }
                else
                {
                    message.Body = htmlBody;
                }
            }
            else
            {
                message.Body = htmlBody;
            }

            using var client = new SmtpClient(_smtpHost, _smtpPort);
            client.Credentials = new NetworkCredential(_senderEmail, _senderPassword);
            client.EnableSsl = true;

            await client.SendMailAsync(message);
            _logger.LogInformation("[EMAIL] Sent successfully to {Email}: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EMAIL] Failed to send email to {Email}: {Subject}", toEmail, subject);
        }
    }

    private static bool IsBase64String(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var s = value.Trim();
        return s.Length % 4 == 0 && System.Text.RegularExpressions.Regex.IsMatch(s, @"^[A-Za-z0-9+/]*={0,2}$");
    }
}
