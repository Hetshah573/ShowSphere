namespace ShowSphere.Application.Features.Theaters.DTOs;

public record TheaterDto(
    Guid Id,
    string Name,
    string Address,
    string City,
    string State,
    string PinCode,
    List<ScreenDto> Screens);

public record TheaterListDto(
    Guid Id,
    string Name,
    string Address,
    string City,
    int ScreenCount);

public record ScreenDto(
    Guid Id,
    string Name,
    int TotalSeats,
    string ScreenType);

public record CreateTheaterRequest(
    string Name,
    string Address,
    string City,
    string State,
    string PinCode);
