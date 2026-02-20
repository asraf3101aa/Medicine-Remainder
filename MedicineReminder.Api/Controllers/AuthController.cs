using MediatR;
using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Application.Common.Models;
using MedicineReminder.Application.Features.Auth.Commands;
using Microsoft.AspNetCore.Mvc;
using MedicineReminder.Domain.Entities;

namespace MedicineReminder.Api.Controllers;

public class AuthController : ApiControllerBase
{
    private readonly ISender _mediator;

    public AuthController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<User>>> Register([FromBody] RegisterCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return Created(result.Data!, "User registered successfully");
        }

        return HandleFailure(result);
    }

    [HttpGet("verify-email")]
    public async Task<ActionResult<ApiResponse<bool>>> VerifyEmail([FromQuery] string userId, [FromQuery] string token)
    {
        var result = await _mediator.Send(new VerifyEmailCommand(userId, token));

        if (result.IsSuccess)
        {
            return Success(true, "Email verified successfully");
        }

        return HandleFailure(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthTokens>>> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return Success(result.Data!, "User logged in successfully");
        }

        return HandleFailure(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthTokens>>> Refresh([FromBody] RefreshCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return Success(result.Data!, "Tokens refreshed successfully");
        }

        return HandleFailure(result);
    }
}
