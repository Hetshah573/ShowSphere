using System.Text.RegularExpressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ShowSphere.Application.Common;
using ShowSphere.Application.Features.Reviews.DTOs;
using ShowSphere.Domain.Entities;

namespace ShowSphere.Application.Features.Reviews.Handlers;

public record CreateReviewCommand(Guid UserId, Guid MovieId, int Rating, string? Comment)
    : IRequest<Result<ReviewDto>>;

public record GetMovieReviewsQuery(Guid MovieId, int Page = 1, int PageSize = 10)
    : IRequest<Result<PagedResult<ReviewDto>>>;

public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, Result<ReviewDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateReviewCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ReviewDto>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.UserId == request.UserId && r.MovieId == request.MovieId, cancellationToken);

        if (existingReview != null)
            return Result<ReviewDto>.Failure("You have already reviewed this movie", 409);

        var movie = await _context.Movies.FindAsync(new object[] { request.MovieId }, cancellationToken);
        if (movie == null)
            return Result<ReviewDto>.Failure("Movie not found", 404);

        if (request.Rating < 1 || request.Rating > 5)
            return Result<ReviewDto>.Failure("Rating must be between 1 and 5", 400);

        // Sanitize comment - strip HTML/scripts, limit length
        var sanitizedComment = SanitizeComment(request.Comment);

        var review = new Review
        {
            UserId = request.UserId,
            MovieId = request.MovieId,
            Rating = request.Rating,
            Comment = sanitizedComment
        };

        _context.Reviews.Add(review);

        // Update movie rating
        var reviews = await _context.Reviews.Where(r => r.MovieId == request.MovieId).ToListAsync(cancellationToken);
        movie.TotalReviews = reviews.Count + 1;
        movie.AverageRating = (reviews.Sum(r => r.Rating) + request.Rating) / (decimal)(reviews.Count + 1);

        await _context.SaveChangesAsync(cancellationToken);

        var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);

        return Result<ReviewDto>.Success(new ReviewDto(
            review.Id, review.UserId, $"{user!.FirstName} {user.LastName}",
            review.Rating, review.Comment, review.CreatedAt), 201);
    }

    private static string? SanitizeComment(string? comment)
    {
        if (string.IsNullOrWhiteSpace(comment)) return null;

        // Strip HTML tags
        var sanitized = Regex.Replace(comment, @"<[^>]*>", string.Empty);
        // Remove script-like patterns
        sanitized = Regex.Replace(sanitized, @"javascript\s*:", string.Empty, RegexOptions.IgnoreCase);
        sanitized = Regex.Replace(sanitized, @"on\w+\s*=", string.Empty, RegexOptions.IgnoreCase);
        // Encode remaining special chars for XSS prevention
        sanitized = sanitized
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#x27;");
        // Trim whitespace and limit to 2000 chars
        sanitized = sanitized.Trim();
        if (sanitized.Length > 2000)
            sanitized = sanitized[..2000];

        return string.IsNullOrWhiteSpace(sanitized) ? null : sanitized;
    }
}

public class GetMovieReviewsQueryHandler : IRequestHandler<GetMovieReviewsQuery, Result<PagedResult<ReviewDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetMovieReviewsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<ReviewDto>>> Handle(GetMovieReviewsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Reviews
            .Include(r => r.User)
            .Where(r => r.MovieId == request.MovieId)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var reviews = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new ReviewDto(
                r.Id, r.UserId, $"{r.User.FirstName} {r.User.LastName}",
                r.Rating, r.Comment, r.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<PagedResult<ReviewDto>>.Success(new PagedResult<ReviewDto>
        {
            Items = reviews,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}
