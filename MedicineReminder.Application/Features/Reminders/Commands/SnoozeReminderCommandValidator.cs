using FluentValidation;

namespace MedicineReminder.Application.Features.Reminders.Commands;

public class SnoozeReminderCommandValidator : AbstractValidator<SnoozeReminderCommand>
{
    public SnoozeReminderCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Id must be greater than 0.");
    }
}
