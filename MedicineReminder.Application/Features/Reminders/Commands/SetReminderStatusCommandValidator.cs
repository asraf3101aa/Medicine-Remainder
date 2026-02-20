using FluentValidation;

namespace MedicineReminder.Application.Features.Reminders.Commands;

public class SetReminderStatusCommandValidator : AbstractValidator<SetReminderStatusCommand>
{
    public SetReminderStatusCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Id must be greater than 0.");
    }
}
