using MediatR;
using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Application.Common.Models;
using MedicineReminder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicineReminder.Application.Features.Reminders.Commands;

public record MarkReminderAsTakenCommand(string Id) : IRequest<ServiceResult<Reminder>>;

public class MarkReminderAsTakenCommandHandler : IRequestHandler<MarkReminderAsTakenCommand, ServiceResult<Reminder>>
{
    private readonly IMedicineReminderDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public MarkReminderAsTakenCommandHandler(IMedicineReminderDbContext context, ICurrentUserService currentUserService, ICacheService cacheService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<ServiceResult<Reminder>> Handle(MarkReminderAsTakenCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var reminder = await _context.Reminders
            .Include(r => r.Medicine)
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.Medicine.UserId == userId, cancellationToken);

        if (reminder == null)
        {
            return ServiceResult<Reminder>.NotFound("Reminder not found or you do not have permission.");
        }

        if (reminder.IsTaken)
        {
            return ServiceResult<Reminder>.Success(reminder, "Reminder already marked as taken.");
        }

        reminder.IsTaken = true;
        reminder.TakenAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveReminderFromHotSetAsync(reminder.Id);

        return ServiceResult<Reminder>.Success(reminder, "Reminder marked as taken successfully.");
    }
}
