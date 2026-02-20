using MediatR;
using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Application.Common.Models;
using MedicineReminder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicineReminder.Application.Features.Reminders.Commands;

public record SetReminderStatusCommand(string Id, bool IsActive) : IRequest<ServiceResult<Reminder>>;

public class SetReminderStatusCommandHandler : IRequestHandler<SetReminderStatusCommand, ServiceResult<Reminder>>
{
    private readonly IMedicineReminderDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public SetReminderStatusCommandHandler(IMedicineReminderDbContext context, ICurrentUserService currentUserService, ICacheService cacheService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<ServiceResult<Reminder>> Handle(SetReminderStatusCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var reminder = await _context.Reminders
            .Include(r => r.Medicine)
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.Medicine.UserId == userId, cancellationToken);

        if (reminder == null)
        {
            return ServiceResult<Reminder>.NotFound("Reminder not found or you do not have permission.");
        }

        reminder.IsActive = request.IsActive;
        await _context.SaveChangesAsync(cancellationToken);

        if (request.IsActive)
        {
            // If re-enabling, check if it should be in hot set
            if (!reminder.IsTaken && reminder.NextReminderUtc <= DateTime.UtcNow.AddHours(24))
            {
                await _cacheService.AddReminderToHotSetAsync(reminder.Id, reminder.NextReminderUtc);
            }
        }
        else
        {
            // If disabling, remove from hot set
            await _cacheService.RemoveReminderFromHotSetAsync(reminder.Id);
        }

        var status = request.IsActive ? "active" : "disabled";
        return ServiceResult<Reminder>.Success(reminder, $"Reminder set to {status}.");
    }
}
