using MedicineReminder.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace MedicineReminder.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApiControllerBase : ControllerBase
{
    protected ActionResult<ApiResponse<T>> InternalServerError<T>(string message, string? error = null)
    {
        return StatusCode(500, ApiResponse<T>.Error(message, error));
    }

    protected ActionResult<ApiResponse<T>> Success<T>(T data, string? message = null)
    {
        return Ok(ApiResponse<T>.Success(data, message));
    }

    protected ActionResult<ApiResponse<T>> Created<T>(T data, string? message = null)
    {
        return StatusCode(201, ApiResponse<T>.Success(data, message));
    }

    protected ActionResult<ApiResponse<T>> BadRequest<T>(string message, Dictionary<string, string[]>? errors = null)
    {
        if (errors != null && errors.Count > 0)
        {
            return BadRequest(ApiResponse<T>.Fail(errors, message));
        }
        return BadRequest(ApiResponse<T>.Error(message));
    }

    protected ActionResult<ApiResponse<T>> NotFound<T>(string message = "Resource not found")
    {
        return NotFound(ApiResponse<T>.Error(message));
    }

    protected ActionResult<ApiResponse<T>> InternalServerError<T>(string message = "An internal server error occurred")
    {
        return StatusCode(500, ApiResponse<T>.Error(message));
    }

    protected ActionResult<ApiResponse<T>> HandleFailure<T>(ServiceResult<T> result)
    {
        if (result.ErrorMessage != null && result.ErrorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound<T>(result.ErrorMessage);
        }

        return InternalServerError<T>(result.ErrorMessage ?? "Something went wrong");
    }
}