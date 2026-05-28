namespace ShowSphere.Domain.Interfaces;

public interface IEmailService
{
    Task SendBookingConfirmationAsync(string toEmail, string userName, string bookingNumber, string movieTitle, string showTime, string seats, decimal amount, string? qrCodeBase64 = null);
    Task SendBookingCancellationAsync(string toEmail, string userName, string bookingNumber, string movieTitle);
    Task SendPasswordResetAsync(string toEmail, string userName, string resetToken);
    Task SendMovieReleaseNotificationAsync(string toEmail, string userName, string movieTitle, string releaseDate);
}
