using FluentValidation;

namespace MedicineReminder.Application.Features.Reminders.Commands;

public class SnoozeReminderCommandValidator : AbstractValidator<SnoozeReminderCommand>
{
    public SnoozeReminderCommandValidator()
    {
        RuleFor(v => v.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0.");
    }
}
