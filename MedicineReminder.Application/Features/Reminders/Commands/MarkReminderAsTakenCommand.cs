using MediatR;
using MedicineReminder.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedicineReminder.Application.Features.Reminders.Commands;

public record MarkReminderAsTakenCommand(int Id) : IRequest<(bool Success, string Message)>;

public class MarkReminderAsTakenCommandHandler : IRequestHandler<MarkReminderAsTakenCommand, (bool Success, string Message)>
{
    private readonly IMedicineDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public MarkReminderAsTakenCommandHandler(IMedicineDbContext context, ICurrentUserService currentUserService, ICacheService cacheService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<(bool Success, string Message)> Handle(MarkReminderAsTakenCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var reminder = await _context.Reminders
            .Include(r => r.Medicine)
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.Medicine.UserId == userId, cancellationToken);

        if (reminder == null)
        {
            return (false, "Reminder not found or you do not have permission.");
        }

        if (reminder.IsTaken)
        {
            return (true, "Reminder already marked as taken.");
        }

        reminder.IsTaken = true;
        reminder.TakenAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveReminderFromHotSetAsync(reminder.Id);

        return (true, "Reminder marked as taken successfully.");
    }
}
