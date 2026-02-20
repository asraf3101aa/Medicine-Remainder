using MediatR;
using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Application.Common.Models;
using MedicineReminder.Application.Features.Auth.Commands;
using Microsoft.AspNetCore.Mvc;

namespace MedicineReminder.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;

    public AuthController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthData>>> Register([FromBody] RegisterCommand command)
    {
        var (data, message, errors) = await _mediator.Send(command);

        if (errors != null && errors.Any())
        {
            return BadRequest(ApiResponse<AuthData>.Error(message, errors));
        }

        return Ok(ApiResponse<AuthData>.Success(data, message));
    }

    [HttpGet("verify-email")]
    public async Task<ActionResult<ApiResponse<bool>>> VerifyEmail([FromQuery] string userId, [FromQuery] string token)
    {
        var (success, message) = await _mediator.Send(new VerifyEmailCommand(userId, token));

        if (!success)
        {
            return BadRequest(ApiResponse<bool>.Error(message));
        }

        return Ok(ApiResponse<bool>.Success(true, message));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthData>>> Login([FromBody] LoginCommand command)
    {
        var (data, message, errors) = await _mediator.Send(command);

        if (errors != null && errors.Any())
        {
            return BadRequest(ApiResponse<AuthData>.Error(message, errors));
        }

        return Ok(ApiResponse<AuthData>.Success(data, message));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthData>>> Refresh([FromBody] RefreshCommand command)
    {
        var (data, message, errors) = await _mediator.Send(command);

        if (errors != null && errors.Any())
        {
            return BadRequest(ApiResponse<AuthData>.Error(message, errors));
        }

        return Ok(ApiResponse<AuthData>.Success(data, message));
    }
}
