namespace MedicineReminder.Application.Common.Models;

public class ServiceResult<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? Message { get; }
    public string? ErrorMessage { get; }
    public IEnumerable<string>? Errors { get; }

    protected ServiceResult(bool isSuccess, T? data, string? message, string? errorMessage, IEnumerable<string>? errors)
    {
        IsSuccess = isSuccess;
        Data = data;
        Message = message;
        ErrorMessage = errorMessage;
        Errors = errors;
    }

    public static ServiceResult<T> Success(T data, string? message = null) => new(true, data, message, null, null);

    public static ServiceResult<T> Failure(string errorMessage) => new(false, default, null, errorMessage, null);

    public static ServiceResult<T> Failure(IEnumerable<string> errors) => new(false, default, null, null, errors);

    public static ServiceResult<T> Failure(string errorMessage, IEnumerable<string> errors) => new(false, default, null, errorMessage, errors);

    public static ServiceResult<T> NotFound(string errorMessage = "Not Found") => new(false, default, null, errorMessage, null);

    public static ServiceResult<T> Unauthorized(string errorMessage = "Unauthorized") => new(false, default, null, errorMessage, null);

    public static ServiceResult<T> InvalidOperation(string errorMessage) => new(false, default, null, errorMessage, null);
}

public class ServiceResult : ServiceResult<object>
{
    private ServiceResult(bool isSuccess, string? message, string? errorMessage, IEnumerable<string>? errors)
        : base(isSuccess, null, message, errorMessage, errors)
    {
    }

    public static ServiceResult Success(string? message = null) => new(true, message, null, null);

    public static new ServiceResult Failure(string errorMessage) => new(false, null, errorMessage, null);

    public static new ServiceResult Failure(IEnumerable<string> errors) => new(false, null, null, errors);

    public static new ServiceResult Failure(string errorMessage, IEnumerable<string> errors) => new(false, null, errorMessage, errors);

    public static new ServiceResult NotFound(string errorMessage = "Not Found") => new(false, null, errorMessage, null);

    public static new ServiceResult Unauthorized(string errorMessage = "Unauthorized") => new(false, null, errorMessage, null);
}
