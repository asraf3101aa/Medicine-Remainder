using MediatR;
using MedicineReminder.Application.Features.Reminders.Commands;
using MedicineReminder.Application.Features.Reminders.Queries;
using MedicineReminder.Application.Common.Models;
using MedicineReminder.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicineReminder.Api.Controllers;

[Authorize]
public class RemindersController : ApiControllerBase
{
    private readonly ISender _mediator;

    public RemindersController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedList<ReminderDto>>>> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] bool? isTaken = null, [FromQuery] bool? isActive = null)
    {
        var result = await _mediator.Send(new GetRemindersQuery { PageNumber = pageNumber, PageSize = pageSize, IsTaken = isTaken, IsActive = isActive });
        if (result.IsSuccess)
        {
            return Success(result.Data!);
        }
        return HandleFailure(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Reminder>>> Schedule(ScheduleReminderCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return Created(result.Data!, "Reminder scheduled successfully.");
        }
        return HandleFailure(result);
    }

    [HttpPut("{id}/taken")]
    public async Task<ActionResult<ApiResponse<Reminder>>> MarkAsTaken(string id)
    {
        var result = await _mediator.Send(new MarkReminderAsTakenCommand(id));
        if (result.IsSuccess)
        {
            return Success(result.Data!, "Reminder marked as taken.");
        }
        return HandleFailure(result);
    }

    [HttpPut("{id}/snooze")]
    public async Task<ActionResult<ApiResponse<Reminder>>> Snooze(string id)
    {
        var result = await _mediator.Send(new SnoozeReminderCommand(id));
        if (result.IsSuccess)
        {
            return Success(result.Data!, "Reminder snoozed.");
        }
        return HandleFailure(result);
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<ApiResponse<Reminder>>> SetStatus(string id, [FromQuery] bool isActive)
    {
        var result = await _mediator.Send(new SetReminderStatusCommand(id, isActive));
        if (result.IsSuccess)
        {
            return Success(result.Data!, $"Reminder status updated to {(isActive ? "active" : "inactive")}.");
        }
        return HandleFailure(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<Reminder>>> Update(string id, UpdateReminderCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest<Reminder>("ID mismatch.");
        }

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return Success(result.Data!, "Reminder updated successfully.");
        }
        return HandleFailure(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<Reminder>>> Delete(string id)
    {
        var result = await _mediator.Send(new DeleteReminderCommand(id));
        if (result.IsSuccess)
        {
            return Success(result.Data!, "Reminder deleted successfully.");
        }
        return HandleFailure(result);
    }
}
