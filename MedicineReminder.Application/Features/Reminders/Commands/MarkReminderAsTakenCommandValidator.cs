using FluentValidation;

namespace MedicineReminder.Application.Features.Reminders.Commands;

public class MarkReminderAsTakenCommandValidator : AbstractValidator<MarkReminderAsTakenCommand>
{
    public MarkReminderAsTakenCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Id must be greater than 0.");
    }
}
