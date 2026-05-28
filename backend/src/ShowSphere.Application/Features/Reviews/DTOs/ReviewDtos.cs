namespace ShowSphere.Application.Features.Reviews.DTOs;

public record ReviewDto(
    Guid Id,
    Guid UserId,
    string UserName,
    int Rating,
    string? Comment,
    DateTime CreatedAt);

public record CreateReviewRequest(
    Guid MovieId,
    int Rating,
    string? Comment);
