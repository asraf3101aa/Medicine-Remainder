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

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, string[]>? Errors { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("error")]
    public object? ErrorInfo { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("errors")]
    public string[]? ErrorMessages { get; set; }

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

    public static ApiResponse<T> NotFound(string? message = "Not Found") => new()
    {
        Status = ApiResponseStatus.Fail,
        Message = message
    };

    public static ApiResponse<T> Error(string message, object? error = null) => new()
    {
        Status = ApiResponseStatus.Error,
        Message = message,
        ErrorInfo = error
    };

    public static ApiResponse<T> Error(string message, string[]? errors) => new()
    {
        Status = ApiResponseStatus.Error,
        Message = message,
        ErrorMessages = errors
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


public class PaginationQuery
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}