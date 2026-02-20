using MediatR;
using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicineReminder.Application.Features.Reminders.Commands;

public record UpdateReminderCommand : IRequest<(bool Success, string Message)>
{
    public int Id { get; init; }
    public DateTime ReminderUtc { get; init; }
    public bool IsActive { get; init; }
}

public class UpdateReminderCommandHandler : IRequestHandler<UpdateReminderCommand, (bool Success, string Message)>
{
    private readonly IMedicineDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public UpdateReminderCommandHandler(IMedicineDbContext context, ICurrentUserService currentUserService, ICacheService cacheService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<(bool Success, string Message)> Handle(UpdateReminderCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var reminder = await _context.Reminders
            .Include(r => r.Medicine)
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.Medicine.UserId == userId, cancellationToken);

        if (reminder == null)
        {
            return (false, "Reminder not found or you do not have permission.");
        }

        bool scheduleChanged = reminder.ReminderUtc != request.ReminderUtc;

        reminder.ReminderUtc = request.ReminderUtc;
        if (scheduleChanged)
        {
            reminder.NextReminderUtc = request.ReminderUtc;
            reminder.IsTaken = false;
            reminder.SnoozeCount = 0;
            // Should IsActive be updated? Yes, per command.
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

        return (true, "Reminder updated successfully.");
    }
}
