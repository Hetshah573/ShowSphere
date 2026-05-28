namespace ShowSphere.Application.Common;

public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public int StatusCode { get; set; }

    public static Result<T> Success(T data, int statusCode = 200) => new()
    {
        IsSuccess = true,
        Data = data,
        StatusCode = statusCode
    };

    public static Result<T> Failure(string error, int statusCode = 400) => new()
    {
        IsSuccess = false,
        Error = error,
        StatusCode = statusCode
    };
}

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;
}
