namespace ShowSphere.Application.Features.Shows.DTOs;

public record ShowDto(
    Guid Id,
    Guid MovieId,
    string MovieTitle,
    Guid ScreenId,
    string ScreenName,
    string TheaterName,
    string City,
    DateTime StartTime,
    DateTime EndTime,
    decimal BasePrice,
    int AvailableSeats,
    int TotalSeats);

public record ShowsByMovieDto(
    string TheaterName,
    string TheaterAddress,
    string City,
    List<ShowTimingDto> Shows);

public record ShowTimingDto(
    Guid ShowId,
    string ScreenName,
    string ScreenType,
    DateTime StartTime,
    decimal MinPrice,
    decimal MaxPrice,
    int AvailableSeats);

public record CreateShowRequest(
    Guid MovieId,
    Guid ScreenId,
    DateTime StartTime,
    decimal BasePrice);
