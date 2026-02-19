using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Medicine.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medicine.Application.Features.Medicines.Queries;

public record GetMedicinesQuery : IRequest<List<MedicineDto>>
{
    public string UserEmail { get; init; } = string.Empty;
}

public class GetMedicinesQueryHandler : IRequestHandler<GetMedicinesQuery, List<MedicineDto>>
{
    private readonly IMedicineDbContext _context;
    private readonly IMapper _mapper;

    public GetMedicinesQueryHandler(IMedicineDbContext context, IMapper _mapper)
    {
        _context = context;
        this._mapper = _mapper;
    }

    public async Task<List<MedicineDto>> Handle(GetMedicinesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Medicines
            .Where(m => m.UserEmail == request.UserEmail)
            .ProjectTo<MedicineDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
