using MediatR;
using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Application.Common.Models;
using MedicineReminder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicineReminder.Application.Features.Reminders.Commands;

public record SnoozeReminderCommand(string Id) : IRequest<ServiceResult<Reminder>>;

public class SnoozeReminderCommandHandler : IRequestHandler<SnoozeReminderCommand, ServiceResult<Reminder>>
{
    private readonly IMedicineReminderDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public SnoozeReminderCommandHandler(IMedicineReminderDbContext context, ICurrentUserService currentUserService, ICacheService cacheService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<ServiceResult<Reminder>> Handle(SnoozeReminderCommand request, CancellationToken cancellationToken)
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
            return ServiceResult<Reminder>.InvalidOperation("Cannot snooze a reminder that has already been taken.");
        }

        if (reminder.SnoozeCount >= 3)
        {
            return ServiceResult<Reminder>.InvalidOperation("Maximum snooze limit reached.");
        }

        reminder.SnoozeCount++;
        reminder.NextReminderUtc = DateTime.UtcNow.AddMinutes(reminder.SnoozeDurationMinutes);

        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.AddReminderToHotSetAsync(reminder.Id, reminder.NextReminderUtc);

        return ServiceResult<Reminder>.Success(reminder, $"Reminder snoozed for {reminder.SnoozeDurationMinutes} minutes.");
    }
}
