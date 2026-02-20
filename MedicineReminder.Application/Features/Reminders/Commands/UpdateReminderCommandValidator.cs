using FluentValidation;

namespace MedicineReminder.Application.Features.Reminders.Commands;

public class UpdateReminderCommandValidator : AbstractValidator<UpdateReminderCommand>
{
    public UpdateReminderCommandValidator()
    {
        RuleFor(v => v.ReminderUtc)
            .NotEmpty().WithMessage("Reminder time is required.");
    }
}
