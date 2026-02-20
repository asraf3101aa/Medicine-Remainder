using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicineReminder.Application.Features.Medicines.Queries;

public record GetMedicinesQuery : IRequest<(PaginatedList<MedicineDto> Data, string Message)>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetMedicinesQueryHandler : IRequestHandler<GetMedicinesQuery, (PaginatedList<MedicineDto> Data, string Message)>
{
    private readonly IMedicineDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetMedicinesQueryHandler(IMedicineDbContext context, IMapper mapper, ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<(PaginatedList<MedicineDto> Data, string Message)> Handle(GetMedicinesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var query = _context.Medicines
            .Where(m => m.UserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(m => m.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<MedicineDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var paginatedList = new PaginatedList<MedicineDto>(items, totalCount, request.PageNumber, request.PageSize);

        return (paginatedList, "Medicines fetched successfully.");
    }
}
