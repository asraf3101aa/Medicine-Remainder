using Asp.Versioning;
using MediatR;
using Medicine.Application.Features.Reminders.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Medicine.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class RemindersController : ControllerBase
{
    private readonly ISender _mediator;

    public RemindersController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<int>> Schedule(ScheduleReminderCommand command)
    {
        return await _mediator.Send(command);
    }
}
