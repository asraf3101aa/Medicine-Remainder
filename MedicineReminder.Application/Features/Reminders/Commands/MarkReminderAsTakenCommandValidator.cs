using FluentValidation;

namespace MedicineReminder.Application.Features.Reminders.Commands;

public class MarkReminderAsTakenCommandValidator : AbstractValidator<MarkReminderAsTakenCommand>
{
    public MarkReminderAsTakenCommandValidator()
    {
        RuleFor(v => v.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0.");
    }
}
