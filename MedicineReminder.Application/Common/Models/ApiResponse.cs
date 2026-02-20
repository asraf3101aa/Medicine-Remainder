using System.Text.Json.Serialization;

namespace MedicineReminder.Application.Common.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ApiResponseStatus
{
    Success,
    Fail,
    Error
}

public class ApiResponse<T>
{
    public ApiResponseStatus Status { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public IDictionary<string, string[]>? Errors { get; set; }
    public object? ErrorDetail { get; set; }

    public static ApiResponse<T> Success(T? data, string? message = null) => new()
    {
        Status = ApiResponseStatus.Success,
        Data = data,
        Message = message
    };

    public static ApiResponse<T> Fail(IDictionary<string, string[]> errors, string? message = "One or more validation errors occurred.") => new()
    {
        Status = ApiResponseStatus.Fail,
        Errors = errors,
        Message = message
    };

    public static ApiResponse<T> Error(string message, object? errorDetail = null) => new()
    {
        Status = ApiResponseStatus.Error,
        Message = message,
        ErrorDetail = errorDetail
    };
}

public class PaginatedList<T>
{
    public IReadOnlyCollection<T> Items { get; }
    public PaginationMetadata Meta { get; }

    public PaginatedList(IReadOnlyCollection<T> items, int count, int pageNumber, int pageSize)
    {
        Items = items;
        Meta = new PaginationMetadata
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = count,
            TotalPages = (int)Math.Ceiling(count / (double)pageSize)
        };
    }
}

public class PaginationMetadata
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
