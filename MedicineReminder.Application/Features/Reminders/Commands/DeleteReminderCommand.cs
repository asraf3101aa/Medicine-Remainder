using MediatR;
using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicineReminder.Application.Features.Reminders.Commands;

public record DeleteReminderCommand(int Id) : IRequest<(bool Success, string Message)>;

public class DeleteReminderCommandHandler : IRequestHandler<DeleteReminderCommand, (bool Success, string Message)>
{
    private readonly IMedicineDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public DeleteReminderCommandHandler(IMedicineDbContext context, ICurrentUserService currentUserService, ICacheService cacheService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<(bool Success, string Message)> Handle(DeleteReminderCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var reminder = await _context.Reminders
            .Include(r => r.Medicine)
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.Medicine.UserId == userId, cancellationToken);

        if (reminder == null)
        {
            return (false, "Reminder not found or you do not have permission.");
        }

        _context.Reminders.Remove(reminder);
        await _context.SaveChangesAsync(cancellationToken);

        // Remove from Redis hot set if it's there
        await _cacheService.RemoveReminderFromHotSetAsync(reminder.Id);

        return (true, "Reminder deleted successfully.");
    }
}
