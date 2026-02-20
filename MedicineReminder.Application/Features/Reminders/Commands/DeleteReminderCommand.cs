using MediatR;
using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Application.Common.Models;
using MedicineReminder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicineReminder.Application.Features.Reminders.Commands;

public record DeleteReminderCommand(string Id) : IRequest<ServiceResult<Reminder>>;

public class DeleteReminderCommandHandler : IRequestHandler<DeleteReminderCommand, ServiceResult<Reminder>>
{
    private readonly IMedicineReminderDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public DeleteReminderCommandHandler(IMedicineReminderDbContext context, ICurrentUserService currentUserService, ICacheService cacheService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<ServiceResult<Reminder>> Handle(DeleteReminderCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var reminder = await _context.Reminders
            .Include(r => r.Medicine)
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.Medicine.UserId == userId, cancellationToken);

        if (reminder == null)
        {
            return ServiceResult<Reminder>.NotFound("Reminder not found or you do not have permission.");
        }

        _context.Reminders.Remove(reminder);
        await _context.SaveChangesAsync(cancellationToken);

        // Remove from Redis hot set if it's there
        await _cacheService.RemoveReminderFromHotSetAsync(reminder.Id);

        return ServiceResult<Reminder>.Success(reminder, "Reminder deleted successfully.");
    }
}
