namespace ShowSphere.Domain.Interfaces;

public interface IEmailService
{
    Task SendBookingConfirmationAsync(string toEmail, string userName, string bookingNumber, string movieTitle, string showTime, string seats, decimal amount, string? qrCodeBase64 = null);
    Task SendBookingCancellationAsync(string toEmail, string userName, string bookingNumber, string movieTitle);
    Task SendPasswordResetAsync(string toEmail, string userName, string resetToken);
    Task SendMovieReleaseNotificationAsync(string toEmail, string userName, string movieTitle, string releaseDate);
    Task SendSchedulerReportAsync(string toEmail, string userName, SchedulerReport report);
}

public class SchedulerReport
{
    public DateTime RunTime { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public int DeactivatedShows { get; set; }
    public int DeletedShows { get; set; }
    public int EligibleMovies { get; set; }
    public int ShowsCreated { get; set; }
    public List<SchedulerShowInfo> ShowDetails { get; set; } = new();
}

public class SchedulerShowInfo
{
    public string MovieTitle { get; set; } = string.Empty;
    public string TheaterName { get; set; } = string.Empty;
    public string ScreenName { get; set; } = string.Empty;
    public string ScreenType { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public decimal BasePrice { get; set; }
}
