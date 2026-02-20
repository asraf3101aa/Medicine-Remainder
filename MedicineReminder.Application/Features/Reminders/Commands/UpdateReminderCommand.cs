using MediatR;
using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Application.Common.Models;
using MedicineReminder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicineReminder.Application.Features.Reminders.Commands;

public record UpdateReminderCommand : IRequest<ServiceResult<Reminder>>
{
    public string Id { get; init; }
    public DateTime ReminderUtc { get; init; }
    public bool IsActive { get; init; }
}

public class UpdateReminderCommandHandler : IRequestHandler<UpdateReminderCommand, ServiceResult<Reminder>>
{
    private readonly IMedicineReminderDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public UpdateReminderCommandHandler(IMedicineReminderDbContext context, ICurrentUserService currentUserService, ICacheService cacheService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<ServiceResult<Reminder>> Handle(UpdateReminderCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var reminder = await _context.Reminders
            .Include(r => r.Medicine)
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.Medicine.UserId == userId, cancellationToken);

        if (reminder == null)
        {
            return ServiceResult<Reminder>.NotFound("Reminder not found or you do not have permission.");
        }

        bool scheduleChanged = reminder.ReminderUtc != request.ReminderUtc;

        reminder.ReminderUtc = request.ReminderUtc;
        if (scheduleChanged)
        {
            reminder.NextReminderUtc = request.ReminderUtc;
            reminder.IsTaken = false;
            reminder.SnoozeCount = 0;
        }
        reminder.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        // Update Redis Cache
        bool shouldBeInHotSet = reminder.IsActive && !reminder.IsTaken && reminder.NextReminderUtc <= DateTime.UtcNow.AddHours(24);

        if (shouldBeInHotSet)
        {
            await _cacheService.AddReminderToHotSetAsync(reminder.Id, reminder.NextReminderUtc);
        }
        else
        {
            await _cacheService.RemoveReminderFromHotSetAsync(reminder.Id);
        }

        return ServiceResult<Reminder>.Success(reminder, "Reminder updated successfully.");
    }
}
