using Asp.Versioning;
using MediatR;
using MedicineReminder.Application.Features.Medicines.Commands;
using MedicineReminder.Application.Features.Medicines.Queries;
using MedicineReminder.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicineReminder.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MedicinesController : ControllerBase
{
    private readonly ISender _mediator;

    public MedicinesController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<int>>> Create(CreateMedicineCommand command)
    {
        var (result, message) = await _mediator.Send(command);
        return Ok(ApiResponse<int>.Success(result, message));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedList<MedicineDto>>>> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var (result, message) = await _mediator.Send(new GetMedicinesQuery { PageNumber = pageNumber, PageSize = pageSize });
        return Ok(ApiResponse<PaginatedList<MedicineDto>>.Success(result, message));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Update(int id, UpdateMedicineCommand command)
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
        var (success, message) = await _mediator.Send(new DeleteMedicineCommand(id));

        if (!success)
        {
            return BadRequest(ApiResponse<bool>.Error(message));
        }

        return Ok(ApiResponse<bool>.Success(true, message));
    }
}
