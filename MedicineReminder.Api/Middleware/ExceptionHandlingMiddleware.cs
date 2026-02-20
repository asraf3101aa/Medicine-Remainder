using System.Net;
using System.Text.Json;
using MedicineReminder.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace MedicineReminder.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        if (exception is FluentValidation.ValidationException validationException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            var response = ApiResponse<object>.Fail(errors);
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
            return;
        }

        object? error = isDevelopment
            ? new { Detail = exception.Message, StackTrace = exception.StackTrace }
            : null;

        if (exception is KeyNotFoundException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            var response = ApiResponse<object>.Error(exception.Message, error);
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
            return;
        }

        if (exception is UnauthorizedAccessException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            var response = ApiResponse<object>.Error("You are not authorized to access this resource.", error);
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
            return;
        }

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var errorResponse = ApiResponse<object>.Error("An unexpected error occurred.", error);

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
    }
}
