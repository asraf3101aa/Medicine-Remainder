using MediatR;
using MedicineReminder.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedicineReminder.Application.Features.Reminders.Commands;

public record SnoozeReminderCommand(int Id) : IRequest<(bool Success, string Message)>;

public class SnoozeReminderCommandHandler : IRequestHandler<SnoozeReminderCommand, (bool Success, string Message)>
{
    private readonly IMedicineDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public SnoozeReminderCommandHandler(IMedicineDbContext context, ICurrentUserService currentUserService, ICacheService cacheService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<(bool Success, string Message)> Handle(SnoozeReminderCommand request, CancellationToken cancellationToken)
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
            return (false, "Cannot snooze a reminder that has already been taken.");
        }

        if (reminder.SnoozeCount >= 3)
        {
            return (false, "Maximum snooze limit reached.");
        }

        reminder.SnoozeCount++;
        reminder.NextReminderUtc = DateTime.UtcNow.AddMinutes(reminder.SnoozeDurationMinutes);

        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.AddReminderToHotSetAsync(reminder.Id, reminder.NextReminderUtc);

        return (true, $"Reminder snoozed for {reminder.SnoozeDurationMinutes} minutes.");
    }
}
