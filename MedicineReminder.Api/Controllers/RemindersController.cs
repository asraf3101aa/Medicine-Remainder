using MediatR;
using MedicineReminder.Application.Features.Reminders.Commands;
using MedicineReminder.Application.Features.Reminders.Queries;
using MedicineReminder.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicineReminder.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RemindersController : ControllerBase
{
    private readonly ISender _mediator;

    public RemindersController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedList<ReminderDto>>>> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] bool? isTaken = null, [FromQuery] bool? isActive = null)
    {
        var (result, message) = await _mediator.Send(new GetRemindersQuery { PageNumber = pageNumber, PageSize = pageSize, IsTaken = isTaken, IsActive = isActive });
        return Ok(ApiResponse<PaginatedList<ReminderDto>>.Success(result, message));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<int>>> Schedule(ScheduleReminderCommand command)
    {
        var (result, message) = await _mediator.Send(command);
        return Ok(ApiResponse<int>.Success(result, message));
    }

    [HttpPut("{id}/taken")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsTaken(int id)
    {
        var (success, message) = await _mediator.Send(new MarkReminderAsTakenCommand(id));

        if (!success)
        {
            return BadRequest(ApiResponse<bool>.Error(message));
        }

        return Ok(ApiResponse<bool>.Success(true, message));
    }

    [HttpPut("{id}/snooze")]
    public async Task<ActionResult<ApiResponse<bool>>> Snooze(int id)
    {
        var (success, message) = await _mediator.Send(new SnoozeReminderCommand(id));

        if (!success)
        {
            return BadRequest(ApiResponse<bool>.Error(message));
        }

        return Ok(ApiResponse<bool>.Success(true, message));
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<ApiResponse<bool>>> SetStatus(int id, [FromQuery] bool isActive)
    {
        var (success, message) = await _mediator.Send(new SetReminderStatusCommand(id, isActive));

        if (!success)
        {
            return BadRequest(ApiResponse<bool>.Error(message));
        }

        return Ok(ApiResponse<bool>.Success(true, message));
    }
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Update(int id, UpdateReminderCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(ApiResponse<bool>.Error("ID mismatch."));
        }

        var (success, message) = await _mediator.Send(command);

        if (!success)
        {
            return BadRequest(ApiResponse<bool>.Error(message));
        }

        return Ok(ApiResponse<bool>.Success(true, message));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        // Note: For Reminders, we might just delete. A specific delete command should be created or reuse existing pattern.
        // But wait, user asked simply for crud endpoints.
        // We don't have a DeleteReminderCommand yet? Let's check.
        // I created DeleteMedicineCommand but not DeleteReminderCommand.
        // I should have created DeleteReminderCommand. I'll create it now.
        // Wait, for this step I am editing controller. I will assume DeleteReminderCommand exists or I will create it in next step.
        // Actually I should have created it. I'll create the command first in next step then update controller.
        // Rethinking: I can update controller now and receive compilation error, then fix it.
        // Or better: Create command first.

        var (success, message) = await _mediator.Send(new DeleteReminderCommand(id));

        if (!success)
        {
            return BadRequest(ApiResponse<bool>.Error(message));
        }

        return Ok(ApiResponse<bool>.Success(true, message));
    }
}
