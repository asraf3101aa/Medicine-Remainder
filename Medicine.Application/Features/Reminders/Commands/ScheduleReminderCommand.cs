using MediatR;
using Medicine.Application.Common.Interfaces;
using Medicine.Domain.Entities;

namespace Medicine.Application.Features.Reminders.Commands;

public record ScheduleReminderCommand : IRequest<int>
{
    public int MedicineId { get; init; }
    public DateTime ReminderUtc { get; init; }
}

public class ScheduleReminderCommandHandler : IRequestHandler<ScheduleReminderCommand, int>
{
    private readonly IMedicineDbContext _context;

    public ScheduleReminderCommandHandler(IMedicineDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(ScheduleReminderCommand request, CancellationToken cancellationToken)
    {
        var reminder = new Reminder
        {
            MedicineId = request.MedicineId,
            ReminderUtc = request.ReminderUtc,
            IsTaken = false
        };

        _context.Reminders.Add(reminder);
        await _context.SaveChangesAsync(cancellationToken);

        return reminder.Id;
    }
}
