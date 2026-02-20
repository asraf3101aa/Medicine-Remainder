using MediatR;
using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Application.Common.Models;
using MedicineReminder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicineReminder.Application.Features.Reminders.Commands;

public record ScheduleReminderCommand : IRequest<ServiceResult<Reminder>>
{
    public string MedicineId { get; init; }
    public DateTime ReminderUtc { get; init; }
}

public class ScheduleReminderCommandHandler : IRequestHandler<ScheduleReminderCommand, ServiceResult<Reminder>>
{
    private readonly IMedicineReminderDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public ScheduleReminderCommandHandler(IMedicineReminderDbContext context, ICurrentUserService currentUserService, ICacheService cacheService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<ServiceResult<Reminder>> Handle(ScheduleReminderCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var medicine = await _context.Medicines
            .FirstOrDefaultAsync(m => m.Id == request.MedicineId && m.UserId == userId, cancellationToken);

        if (medicine == null)
        {
            return ServiceResult<Reminder>.NotFound("Medicine not found or you do not have permission.");
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

        return ServiceResult<Reminder>.Success(reminder, "Reminder scheduled successfully.");
    }
}
