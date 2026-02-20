using FluentValidation;

namespace MedicineReminder.Application.Features.Reminders.Commands;

public class ScheduleReminderCommandValidator : AbstractValidator<ScheduleReminderCommand>
{
    public ScheduleReminderCommandValidator()
    {
        RuleFor(v => v.MedicineId)
            .NotEmpty().WithMessage("Medicine ID is required.")
            .GreaterThan(0).WithMessage("Invalid Medicine ID.");

        RuleFor(v => v.ReminderUtc)
            .NotEmpty().WithMessage("Reminder time is required.")
            .Must(time => time > DateTime.UtcNow).WithMessage("Reminder time must be in the future.");
    }
}
