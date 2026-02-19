using Asp.Versioning;
using MediatR;
using Medicine.Application.Features.Medicines.Commands;
using Medicine.Application.Features.Medicines.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Medicine.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class MedicinesController : ControllerBase
{
    private readonly ISender _mediator;

    public MedicinesController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateMedicineCommand command)
    {
        return await _mediator.Send(command);
    }

    [HttpGet]
    public async Task<ActionResult<List<MedicineDto>>> Get([FromQuery] string userEmail)
    {
        return await _mediator.Send(new GetMedicinesQuery { UserEmail = userEmail });
    }
}
