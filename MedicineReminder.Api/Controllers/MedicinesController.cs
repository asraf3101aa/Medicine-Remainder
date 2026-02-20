using MediatR;
using MedicineReminder.Application.Features.Medicines.Commands;
using MedicineReminder.Application.Features.Medicines.Queries;
using MedicineReminder.Application.Common.Models;
using MedicineReminder.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicineReminder.Api.Controllers;

[Authorize]
public class MedicinesController : ApiControllerBase
{
    private readonly ISender _mediator;

    public MedicinesController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Medicine>>> Create(CreateMedicineCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return Created(result.Data!, result.Message);
        }
        return HandleFailure(result);
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedList<MedicineDto>>>> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetMedicinesQuery { PageNumber = pageNumber, PageSize = pageSize });
        if (result.IsSuccess)
        {
            return Success(result.Data!);
        }
        return HandleFailure(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<Medicine>>> Update(string id, UpdateMedicineCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest<Medicine>("ID mismatch.");
        }

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return Success(result.Data!, result.Message);
        }
        return HandleFailure(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<Medicine>>> Delete(string id)
    {
        var result = await _mediator.Send(new DeleteMedicineCommand(id));
        if (result.IsSuccess)
        {
            return Success(result.Data!, "Medicine deleted successfully.");
        }
        return HandleFailure(result);
    }
}
