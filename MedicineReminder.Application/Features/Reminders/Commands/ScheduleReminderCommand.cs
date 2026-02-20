using MediatR;
using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicineReminder.Application.Features.Reminders.Commands;

public record ScheduleReminderCommand : IRequest<(int Data, string Message)>
{
    public int MedicineId { get; init; }
    public DateTime ReminderUtc { get; init; }
}

public class ScheduleReminderCommandHandler : IRequestHandler<ScheduleReminderCommand, (int Data, string Message)>
{
    private readonly IMedicineDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public ScheduleReminderCommandHandler(IMedicineDbContext context, ICurrentUserService currentUserService, ICacheService cacheService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<(int Data, string Message)> Handle(ScheduleReminderCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var medicine = await _context.Medicines
            .FirstOrDefaultAsync(m => m.Id == request.MedicineId && m.UserId == userId, cancellationToken);

        if (medicine == null)
        {
            throw new KeyNotFoundException("Medicine not found or you do not have permission.");
        }

        var reminder = new Reminder
        {
            MedicineId = request.MedicineId,
            ReminderUtc = request.ReminderUtc,
            NextReminderUtc = request.ReminderUtc,
            IsTaken = false
        };

        _context.Reminders.Add(reminder);
        await _context.SaveChangesAsync(cancellationToken);

        // Check if reminder is due within 24 hours
        if (reminder.ReminderUtc <= DateTime.UtcNow.AddHours(24))
        {
            await _cacheService.AddReminderToHotSetAsync(reminder.Id, reminder.ReminderUtc);
        }

        return (reminder.Id, "Reminder scheduled successfully.");
    }
}
