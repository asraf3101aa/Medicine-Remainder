using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicineReminder.Application.Features.Reminders.Queries;

public record GetRemindersQuery : IRequest<ServiceResult<PaginatedList<ReminderDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public bool? IsTaken { get; init; }
    public bool? IsActive { get; init; }
}

public class GetRemindersQueryHandler : IRequestHandler<GetRemindersQuery, ServiceResult<PaginatedList<ReminderDto>>>
{
    private readonly IMedicineReminderDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetRemindersQueryHandler(IMedicineReminderDbContext context, IMapper mapper, ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<ServiceResult<PaginatedList<ReminderDto>>> Handle(GetRemindersQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var query = _context.Reminders
            .Include(r => r.Medicine)
            .Where(r => r.Medicine.UserId == userId);

        if (request.IsTaken.HasValue)
        {
            query = query.Where(r => r.IsTaken == request.IsTaken.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(r => r.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.ReminderUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<ReminderDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var paginatedList = new PaginatedList<ReminderDto>(items, totalCount, request.PageNumber, request.PageSize);

        return ServiceResult<PaginatedList<ReminderDto>>.Success(paginatedList, "Reminders fetched successfully.");
    }
}
